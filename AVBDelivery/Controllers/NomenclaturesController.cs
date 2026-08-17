using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;

namespace AVBDelivery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NomenclaturesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        public static NLog.Logger NLogger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public NomenclaturesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Nomenclatures
        [HttpGet]
        [Authorize]
        [Authorize(Roles = "integrator,client")]
        public async Task<ActionResult<IEnumerable<Nomenclature>>> GetNomenclature()
        {
            var t= await _context.Nomenclature.Include(x => x.ProductGroup).ThenInclude(x => x.Products).Include(x => x.Products).ToListAsync();
            return t;
            
        }


        // POST: api/Nomenclatures
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        //[Authorize(Roles = "integrator")]
        public async Task<ActionResult<Nomenclature>> PostNomenclature(Nomenclature nomenclature)
        {
            try
            {
                NLogger.Info($"Тянем номенклатуру из БД.");

                // Тянем продукты, которые не созданы вручную (productInManualMode)
                var prod = await _context.Products.ToListAsync();
                NLogger.Info($"Получили товары.");

                // Тянем группы, которые не созданы вручную (groupInManualMode)
                var group = await _context.ProductGroups.AsNoTracking().ToListAsync();
                NLogger.Info($"Получили группы.");

                var t = await _context.Nomenclature.FirstOrDefaultAsync();
                NLogger.Info($"Получили номенклатуру.");



                    NLogger.Info($"Полученная номенклатура не пустая. Приступаем к чистке.");

                    if (!(group == nomenclature.ProductGroup && prod == nomenclature.Products))
                    {

                        //Удаляем продукты
                        if (prod != null)
                        {
                            NLogger.Info($"Удаляем продукты. Их {prod.Count}.");
                            _context.Products.RemoveRange(prod);
                            await _context.SaveChangesAsync();
                            NLogger.Info($"Продукты удалены.");

                        }

                        //Удаляем группы
                        group = await _context.ProductGroups.Include(x => x.Products).ToListAsync();
                        if (group != null)
                        {
                            NLogger.Info($"Удаляем группы. Их {group.Count}");
                            foreach (var item in group)
                            {
                                if (item.Products.Count == 0)
                                {
                                    NLogger.Info($"Удаляем группу \"{item.GroupName}\", т.к. в ней нет ручной номенклатуры.");
                                    _context.ProductGroups.RemoveRange(item);
                                }
                            }
                            
                            await _context.SaveChangesAsync();
                        }

                        //Удаляем номенклатуру
                        NLogger.Info($"Удаляем номенклатуру.");
                        //_context.Nomenclature.RemoveRange(t);
                        //await _context.SaveChangesAsync();


                        //_context.Nomenclature.Remove(t);
                        //await _context.SaveChangesAsync();
                        //_context.Products.RemoveRange(t.Products);
                        //await _context.SaveChangesAsync();
                        //_context.Productgroups.RemoveRange(t.ProductGroup);
                        //await _context.SaveChangesAsync();
                    }
              


                NLogger.Info($"Добавляем новую номенклатуру.");
                group = await _context.ProductGroups.ToListAsync();
                foreach (var item in nomenclature.ProductGroup)
                {
                    try
                    {
                        NLogger.Info($"Добавляем (обновляем) группу \"{item.GroupName}\"");

                        var tempGroup = await _context.ProductGroups.Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == item.Id);
                        if (tempGroup != null)
                        {
                            if (item != tempGroup)
                            {
                                NLogger.Info($"Группы отличаются. Будем обновлять.");

                                if (tempGroup != null)
                                {
                                    NLogger.Info($"Группа \"{item.GroupName}\" существует, обновляем её свойства.");
                                    NLogger.Info($"В группе \"{item.GroupName}\" {tempGroup.Products.Count} продуктов, добавляем ещё {item.Products.Count}");

                                    foreach (var newProduct in item.Products)
                                    {
                                        var tempProd = tempGroup.Products.FirstOrDefault(x => x.Id == newProduct.Id);
                                        // Обновляем продукт
                                        if (tempProd != null)
                                        {
                                            NLogger.Info($"В группе \"{item.GroupName}\" обновляем продукт \"{tempProd.Name}\".");

                                            tempProd.Name = newProduct.Name;
                                            tempProd.Price = newProduct.Price;
                                            tempProd.Type = newProduct.Type;
                                            tempProd.IsActive = newProduct.IsActive;
                                            tempProd.ParentGroupName = newProduct.ParentGroupName;

                                            _context.Update(tempProd);
                                            NLogger.Info($"Продукт \"{tempProd.Name}\" обновлён.");

                                        }
                                        //Иначе добавляем
                                        else
                                        {
                                            NLogger.Info($"В группе \"{item.GroupName}\" добавляем продукт \"{newProduct.Name}\".");
                                            tempGroup.Products.Add(newProduct);
                                        }
                                    }
                                    //tempGroup.Products.AddRange(item.Products);
                                    tempGroup.GroupName = item.GroupName;

                                    _context.Update(tempGroup);
                                    NLogger.Info($"Свойства группы \"{item.GroupName}\" обновлены.");
                                }
                                else
                                {
                                    NLogger.Info($"Группы \"{item.GroupName}\" не существует, добавляем её.");
                                    await _context.ProductGroups.AddRangeAsync(item);
                                }
                                NLogger.Info($"Сохраняем изменения для группы \"{item.GroupName}\".");
                                await _context.SaveChangesAsync();
                                NLogger.Info($"Изменения для группы \"{item.GroupName}\" сохранены.");
                            }
                            else
                            {
                                NLogger.Info($"Группы идентичны. Обновление не требуется.");
                            }
                        }
                        else
                        {
                            NLogger.Info($"Добавляем группу \"{item.GroupName}\", т.к. её не было ранее.");

                            await _context.AddAsync(item);
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogger.Error(ex, $"Произошла ошибка при обновлении группы \"{item.GroupName}\".\n{ex.Message}\n{ex.InnerException}");
                    }

                }
                
                //await _context.Productgroup.AddRangeAsync(nomenclature.ProductGroup);
                //await _context.Nomenclature.AddAsync(nomenclature);

                //if (t.Count == 0)
                //{
                //    await _context.Nomenclature.AddAsync(nomenclature);
                //}
                //else
                //{                
                //    _context.Nomenclature.Update(nomenclature);
                //}
                //await _context.SaveChangesAsync();
                //_context.RemoveRange(t);
                //await _context.SaveChangesAsync();

                return CreatedAtAction("GetNomenclature", new { id = nomenclature.Id }, nomenclature);
            }
            catch (Exception ex)
            {
                NLogger.Error(ex, $"Произошла ошибка при обновлении номенклатуры.\n{ex.Message}\n{ex.InnerException}");
                return null;
            }

        }



        private bool NomenclatureExists(int id)
        {
            return _context.Nomenclature.Any(e => e.Id == id);
        }
    }
}
