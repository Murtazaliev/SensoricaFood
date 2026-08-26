using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AVBDelivery.Features.Orders.ConfirmUpload;
using AVBDelivery.Features.Orders.CreateOrder;
using AVBDelivery.Features.Orders.GetCreateOrderData;
using AVBDelivery.Features.Orders.GetOrderDetails;
using AVBDelivery.Features.Orders.GetOrders;
using AVBDelivery.Features.Orders.GetOrdersReport;
using AVBDelivery.Features.Orders.GetUploadFormData;
using AVBDelivery.Features.Orders.ParseUploadFile;
using AVBDelivery.Features.Orders.RepeatOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AVBDelivery.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "client")]
        public async Task<IActionResult> Index(string? startDate, string? endDate, string? organizationId, int page = 1)
        {
            var result = await _mediator.Send(new GetOrdersQuery(startDate, endDate, organizationId, page));

            bool? success = null;
            if (TempData.ContainsKey("OrderResult.Success"))
            {
                success = string.Equals(TempData["OrderResult.Success"]?.ToString(), "true", StringComparison.OrdinalIgnoreCase);
            }
            ViewBag.OrderSuccess = success;
            ViewBag.OrderMessage = TempData["OrderResult.Message"]?.ToString();
            ViewBag.OrderId = TempData["OrderResult.OrderId"]?.ToString();

            return View(result);
        }

        [Authorize(Roles = "admin, operator")]
        public async Task<IActionResult> Report()
        {
            return View(await _mediator.Send(new GetOrdersReportQuery()));
        }

        [Authorize(Roles = "client")]
        public async Task<IActionResult> Details(int? id)
        {
            var result = await _mediator.Send(new GetOrderDetailsQuery(id));
            if (result == null) return NotFound();
            return View(result);
        }

        [Authorize(Roles = "client")]
        public async Task<IActionResult> Create()
        {
            var result = await _mediator.Send(new GetCreateOrderDataQuery());

            if (TempData.TryGetValue("NotAdded", out object? value))
            {
                try
                {
                    result.NotAddedItems = JsonSerializer.Deserialize<List<string>>(value as string ?? "[]");
                }
                catch { }
            }

            return View(result);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> Create(ViewModels.OrderCreateViewModel orderCreate)
        {
            if (!ModelState.IsValid)
            {
                return View(orderCreate);
            }

            var result = await _mediator.Send(new CreateOrderCommand(orderCreate));

            TempData["OrderResult.Success"] = result.Success;
            TempData["OrderResult.Message"] = result.Message;
            TempData["OrderResult.OrderId"] = result.OrderId;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> RepeatOrder(int? id)
        {
            var result = await _mediator.Send(new RepeatOrderCommand(id));
            if (result.NotAddedItems?.Count > 0)
            {
                TempData["NotAdded"] = JsonSerializer.Serialize(result.NotAddedItems);
            }
            return RedirectToAction("Create");
        }

        [Authorize(Roles = "client")]
        public async Task<IActionResult> Upload()
        {
            var result = await _mediator.Send(new GetUploadFormDataQuery());
            return View(result);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var result = await _mediator.Send(new ParseUploadFileQuery(file));
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage!);
                return View(new ViewModels.OrderUploadPreviewViewModel());
            }
            return View(result.Preview);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> ConfirmUpload()
        {
            var result = await _mediator.Send(new ConfirmUploadCommand());
            TempData["OrderResult.Success"] = result.Success;
            TempData["OrderResult.Message"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
