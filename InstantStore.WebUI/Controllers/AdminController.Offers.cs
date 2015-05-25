using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.ViewModels.Factories;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels.Helpers;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController : Controller
    {
        private static Dictionary<char, bool> offerStatusMap = new Dictionary<char, bool>
        {
            { 'a', true },
            { 'b', false }
        };

        private static Dictionary<char, string> offerStatusLabelMap = new Dictionary<char, string>
        {
            { 'a', StringResource.OfferStatus_Active },
            { 'b', StringResource.OfferStatus_Disabled }
        };

        public ActionResult Offers(char t = 'a', int o = 0, int c = 50)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, repository);

            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Offers);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;

            this.ViewData["OffersHeaderViewModel"] = new TabControlViewModel
            {
                Tabs = offerStatusMap.Select(key => CreateOfferStatusHeader(key, t)).ToList()
            };

            bool showActive;
            if (!offerStatusMap.TryGetValue(t, out showActive))
            {
                showActive = true;
            }

            using (var context = new InstantStoreDataContext())
            {
                Func<Offer, bool> selector = showActive
                    ? (Func<Offer, bool>)((Offer offer) => offer.IsActive)
                    : (Func<Offer, bool>)((Offer offer) => !offer.IsActive);

                this.ViewData["OffersTableViewModel"] = new TableViewModel
                {
                    Header = new List<TableCellViewModel>
                    {
                        new TableCellViewModel(StringResource.offerTableHeaderName),
                        new TableCellViewModel(StringResource.offerTableHeaderThreshold),
                        new TableCellViewModel(StringResource.offerTableHeaderDiscount),
                        new TableCellViewModel(string.Empty)
                    },
                    Rows = context.Offers
                        .Where(selector)
                        .OrderByDescending(offer => offer.Name)
                        .Skip(o)
                        .Take(c)
                        .Select(ConvertOfferToTableRow)
                        .ToList(),
                    RowClickAction = new NavigationLink("Offer"),
                    Pagination = new PaginationViewModel(c, o, context.Offers.Count(selector))
                    {
                        Link = new NavigationLink("Offers", "Admin")
                        {
                            Parameters = new { t = t }
                        }
                    }
                };
            }

            return this.View();
        }

        private BreadcrumbItemViewModel CreateOfferStatusHeader(KeyValuePair<char, bool> data, char current)
        {
            return new BreadcrumbItemViewModel
            {
                IsActive = data.Key == current,
                Name = offerStatusLabelMap[data.Key],
                Link = new NavigationLink("Offers", "Admin") { Parameters = new { t = data.Key } }
            };
        }

        private TableRowViewModel ConvertOfferToTableRow(Offer offer)
        {
            return new TableRowViewModel
            {
                Id = offer.VersionId.ToString(),
                Cells = new List<TableCellViewModel>
                {
                    new TableCellViewModel(offer.Name),
                    new TableCellViewModel(new CurrencyString(offer.ThresholdPriceValue, offer.Currency.Text).ToString()),
                    new TableCellViewModel(offer.DiscountType == (int)OfferDiscountType.Percent 
                        ? string.Format(StringResource.PercentDiscountFormat, offer.DiscountValue)
                        : new CurrencyString(offer.DiscountValue, offer.Currency.Text).ToString()),
                    new TableCellViewModel(new NavigationLink("DeleteOffer") { PageId = offer.Id, Text = StringResource.admin_Delete })
                }
            };
        }

        public ActionResult Offer(Guid? id)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, repository);
            if (user == null || !user.IsAdmin)
            {
                return this.HttpNotFound();
            }

            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Offers);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;

            OfferViewModel viewModel;

            using (var context = new InstantStoreDataContext())
            {
                if (id != null && id != Guid.Empty)
                {
                        var offer = context.Offers.FirstOrDefault(x => x.VersionId == id.Value);
                        if (offer == null)
                        {
                            return this.HttpNotFound();
                        }

                        viewModel = new OfferViewModel
                        {
                            Id = offer.VersionId,
                            IsActive = offer.IsActive,
                            Name = offer.Name,
                            IsMultiApply = offer.MultiApply,
                            Priority = offer.Priority,
                            Type = (OfferDiscountType)offer.DiscountType,
                            CurrencyId = offer.CurrencyId,
                            ThresholdPrice = offer.ThresholdPriceValue,
                            Discount = offer.DiscountValue
                        };
                }
                else
                {
                    viewModel = new OfferViewModel
                    {
                        IsActive = true
                    };
                }

                this.ViewData["CurrencyList"] = context.Currencies.Select(
                    currency => 
                    new SelectListItem
                    {
                        Text = currency.Text,
                        Value = currency.Id.ToString()
                    }).ToList();

                this.ViewData["DiscountTypeList"] = new[] { OfferDiscountType.Percent, OfferDiscountType.LumpSum }.Select(
                    type =>
                    new SelectListItem
                    {
                        Text = type == OfferDiscountType.Percent
                        ? StringResource.offerDetailsDiscountType_Percent
                        : StringResource.offerDetailsDiscountType_LumpSum,
                        Value = type.ToString()
                    }).ToList();
            }

            return this.View(viewModel);
        }

        [HttpPost]
        public ActionResult Offer(OfferViewModel viewModel)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, repository);
            if (user == null || !user.IsAdmin)
            {
                return this.HttpNotFound();
            }

            if (this.ModelState.IsValid)
            {
                if (viewModel.Type == OfferDiscountType.Percent && viewModel.Discount >= 100)
                {
                    this.ModelState.AddModelError(string.Empty, StringResource.offer_ErrorDiscountPercentRange);
                }
                else
                {
                    using (var context = new InstantStoreDataContext())
                    {
                        if (viewModel.Id == Guid.Empty)
                        {
                            context.Offers.InsertOnSubmit(new Offer
                            {
                                Id = Guid.NewGuid(),
                                Name = viewModel.Name,
                                IsActive = viewModel.IsActive,
                                MultiApply = viewModel.IsMultiApply,
                                Priority = viewModel.Priority,
                                CurrencyId = viewModel.CurrencyId,
                                DiscountType = (int)viewModel.Type,
                                DiscountValue = viewModel.Discount,
                                ThresholdPriceValue = viewModel.ThresholdPrice,
                                Version = 1,
                                VersionId = Guid.NewGuid()
                            });

                            context.SubmitChanges();
                        }
                        else
                        {
                            // check if offer is being used in any order.
                            var offer = context.Offers.FirstOrDefault(o => o.VersionId == viewModel.Id);
                            if (offer == null)
                            {
                                return this.HttpNotFound();
                            }
                            
                            bool increaseVersion = context.Orders.Any(order => order.OfferId == viewModel.Id);
                            if (increaseVersion)
                            {
                                context.Offers.InsertOnSubmit(new Offer
                                {
                                    Id = offer.Id,
                                    Name = viewModel.Name,
                                    IsActive = viewModel.IsActive,
                                    MultiApply = viewModel.IsMultiApply,
                                    Priority = viewModel.Priority,
                                    CurrencyId = viewModel.CurrencyId,
                                    DiscountType = (int)viewModel.Type,
                                    DiscountValue = viewModel.Discount,
                                    ThresholdPriceValue = viewModel.ThresholdPrice,
                                    Version = offer.Version + 1,
                                    VersionId = Guid.NewGuid()
                                });
                            }
                            else
                            {
                                offer.Name = viewModel.Name;
                                offer.IsActive = viewModel.IsActive;
                                offer.MultiApply = viewModel.IsMultiApply;
                                offer.Priority = viewModel.Priority;
                                offer.CurrencyId = viewModel.CurrencyId;
                                offer.DiscountType = (int)viewModel.Type;
                                offer.DiscountValue = viewModel.Discount;
                                offer.ThresholdPriceValue = viewModel.ThresholdPrice;
                            }

                            context.SubmitChanges();
                        }
                    }

                    return this.RedirectToAction("Offers");
                }
            }
            
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Offers);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;

            using (var context = new InstantStoreDataContext())
            {
                this.ViewData["CurrencyList"] = context.Currencies.Select(
                    currency =>
                    new SelectListItem
                    {
                        Text = currency.Text,
                        Value = currency.Id.ToString()
                    }).ToList();
            }

            this.ViewData["DiscountTypeList"] = new[] { OfferDiscountType.Percent, OfferDiscountType.LumpSum }.Select(
                type =>
                new SelectListItem
                {
                    Text = type == OfferDiscountType.Percent
                    ? StringResource.offerDetailsDiscountType_Percent
                    : StringResource.offerDetailsDiscountType_LumpSum,
                    Value = type.ToString()
                }).ToList();

            return this.View(viewModel);
        }

        public ActionResult DeleteOffer(Guid id)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, repository);
            if (user == null || !user.IsAdmin || id == Guid.Empty)
            {
                return this.HttpNotFound();
            }


            using (var context = new InstantStoreDataContext())
            {
                var offer = context.Offers.FirstOrDefault(x => x.Id == id);
                if (offer != null)
                {
                    context.Offers.DeleteOnSubmit(offer);
                    context.SubmitChanges();
                }
            }

            return this.RedirectToAction("Offers");
        }
    }
}
