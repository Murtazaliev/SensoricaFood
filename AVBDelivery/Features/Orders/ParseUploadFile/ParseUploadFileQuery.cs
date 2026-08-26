using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using Microsoft.AspNetCore.Http;
using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AVBDelivery.Features.Orders.ParseUploadFile
{
    public record ParseUploadFileQuery(IFormFile File) : IRequest<ParseUploadFileResult>;

    public class ParseUploadFileResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public OrderUploadPreviewViewModel? Preview { get; set; }
    }

    public class ParseUploadFileQueryHandler : IRequestHandler<ParseUploadFileQuery, ParseUploadFileResult>
    {
        private readonly ApplicationContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IDistributedCache _cache;

        public ParseUploadFileQueryHandler(
            ApplicationContext context,
            ICurrentUserService currentUser,
            IDistributedCache cache)
        {
            _context = context;
            _currentUser = currentUser;
            _cache = cache;
        }

        public async Task<ParseUploadFileResult> Handle(ParseUploadFileQuery request, CancellationToken ct)
        {
            var user = await _currentUser.GetUserAsync();
            var file = request.File;

            if (file == null || file.Length == 0)
            {
                return new ParseUploadFileResult { IsSuccess = false, ErrorMessage = OrderConstants.Messages.FileNotSelected };
            }

            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, ct);
            stream.Position = 0;

            using var package = new OfficeOpenXml.ExcelPackage(stream);
            var ws = package.Workbook.Worksheets.FirstOrDefault();
            if (ws == null)
            {
                return new ParseUploadFileResult { IsSuccess = false, ErrorMessage = OrderConstants.Messages.FileNoSheets };
            }

            int totalCol = ws.Dimension?.End?.Column ?? 0;
            int totalRow = ws.Dimension?.End?.Row ?? 0;

            if (totalCol < 4 || totalRow < 2)
            {
                return new ParseUploadFileResult { IsSuccess = false, ErrorMessage = OrderConstants.Messages.FileNoData };
            }

            int itogoCol = 0;
            for (int c = 1; c <= totalCol; c++)
            {
                var val = ws.Cells[1, c].Text?.Trim();
                if (val != null && (val.Equals(OrderConstants.Xlsx.TotalColumnRu, StringComparison.OrdinalIgnoreCase) || val.Equals(OrderConstants.Xlsx.TotalColumnEn, StringComparison.OrdinalIgnoreCase)))
                {
                    itogoCol = c;
                    break;
                }
            }

            int lastDataCol = itogoCol > 0 ? itogoCol - 1 : totalCol;

            var locationHeaders = new List<(int Col, string Header)>();
            for (int c = OrderConstants.Xlsx.FirstDataColumn; c <= lastDataCol; c++)
            {
                var header = ws.Cells[1, c].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    locationHeaders.Add((c, header));
                }
            }

            if (locationHeaders.Count == 0)
            {
                return new ParseUploadFileResult { IsSuccess = false, ErrorMessage = OrderConstants.Messages.NoAddressColumns };
            }

            var allProducts = await _context.Products.Where(p => p.IsActive).ToListAsync(ct);

            var userContacts = await _context.Contacts
                .Where(c => c.UserId == user!.Id)
                .Include(c => c.Organizations)
                .ToListAsync(ct);
            var userOrgs = userContacts.SelectMany(c => c.Organizations ?? new List<Organization>()).ToList();

            var model = new OrderUploadPreviewViewModel
            {
                SheetName = ws.Name,
                UnmatchedNames = new List<string>()
            };

            foreach (var loc in locationHeaders)
            {
                var matchedOrg = userOrgs.FirstOrDefault(o =>
                    !string.IsNullOrEmpty(o.Comment) && o.Comment.Contains(loc.Header));

                var group = new OrderGroupByLocation
                {
                    ColumnHeader = loc.Header,
                    OrganizationId = matchedOrg?.OrganizationId,
                    OrganizationName = matchedOrg?.Name,
                    OrganizationFound = matchedOrg != null
                };

                for (int r = OrderConstants.Xlsx.FirstDataRow; r <= totalRow; r++)
                {
                    var nameCell = ws.Cells[r, 1].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(nameCell)) continue;
                    if (nameCell.Equals(OrderConstants.Xlsx.TotalColumnRu, StringComparison.OrdinalIgnoreCase)) continue;

                    var qtyCell = ws.Cells[r, loc.Col].Value;
                    double qty = 0;
                    if (qtyCell != null)
                    {
                        if (qtyCell is double d) qty = d;
                        else double.TryParse(qtyCell.ToString(), out qty);
                    }

                    var product = allProducts.FirstOrDefault(p => p.Name == nameCell)
                                  ?? allProducts.FirstOrDefault(p => p.Name.Contains(nameCell) || nameCell.Contains(p.Name));

                    group.Items.Add(new MatchedOrderItem
                    {
                        FileName = nameCell,
                        ProductId = product?.Id,
                        ProductName = product?.Name,
                        Price = product?.Price,
                        MeasureUnit = product?.MeasureUnit,
                        AmoCrmId = product?.AmoCrmId,
                        Quantity = qty,
                        IsFound = product != null
                    });

                    if (product == null && !model.UnmatchedNames.Contains(nameCell))
                    {
                        model.UnmatchedNames.Add(nameCell);
                    }
                }

                model.OrderGroups.Add(group);
            }

            var jsonData = System.Text.Json.JsonSerializer.Serialize(model);
            await _cache.SetStringAsync(string.Format(OrderConstants.CacheKeys.UploadPreview, user!.Id), jsonData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = OrderConstants.CacheExpiration.UploadPreviewTtl
            }, ct);

            return new ParseUploadFileResult { IsSuccess = true, Preview = model };
        }
    }
}
