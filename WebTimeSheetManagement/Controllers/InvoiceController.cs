using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using Intuit.Ipp.OAuth2PlatformClient;
using DocumentFormat.OpenXml.EMMA;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using DocumentFormat.OpenXml.Office2010.Excel;
using Intuit.Ipp.DataService;
using System.Data.Entity;

namespace WebTimeSheetManagement.Controllers
{
    public class InvoiceController : Controller
    {
        public string ClientID;
        public string ClientSecret;
        public string RedirectUrl = "https://devices.pythonanywhere.com/";
        public string Environment = "sandbox";

        // GET: Invoice
        [HttpGet]
        public ActionResult Invoice()
        {
            Console.WriteLine(GetServiceContext());
            return View();
        }
        public static string GetAccessToken()
        {
            var oauth2Client = new OAuth2Client("ABENc6Fvovge9epVWBD7ofQ6sDrEfxyoAoNUZadtKGIPHi5w2w",
                    "eMu0SxVabkfJoQZo5nLsal3Tf49R9KpgBxRhdCcC",
                    // Redirect not used but matches entry for app
                    "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl",
                    "sandbox"); // environment is “sandbox” or “production”

            var previousRefreshToken = "AB11682085318ddJxlk501UWDdl6F3zuZEF4yVP6GfZNYiLBoM";
            var tokenResp = oauth2Client.RefreshTokenAsync(previousRefreshToken);
            tokenResp.Wait();
            var data = tokenResp.Result;

            if (!String.IsNullOrEmpty(data.Error) || String.IsNullOrEmpty(data.RefreshToken) ||
                  String.IsNullOrEmpty(data.AccessToken))
            {
                throw new Exception("Refresh token failed - " + data.Error);
            }

            // If we've got a new refresh_token store it in the file
            if (previousRefreshToken != data.RefreshToken)
            {
                Console.WriteLine("Writing new refresh token : " + data.RefreshToken);
                previousRefreshToken = data.RefreshToken;

            }
                return data.AccessToken;
        }

        static public ServiceContext GetServiceContext()
        {
            var accessToken = GetAccessToken(); // Code from above
            var oauthValidator = new OAuth2RequestValidator(accessToken);


            ServiceContext context = new ServiceContext("4620816365247727380", IntuitServicesType.QBO, oauthValidator);
            context.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";
            var service = new DataService(context);


            context.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";
            var tQuery = service.FindAll(new Customer());
            addInvoice();
            return context;
        }

        public static void addInvoice()
        {
            var accessToken = GetAccessToken(); // Code from above
            var oauthValidator = new OAuth2RequestValidator(accessToken);


            ServiceContext context = new ServiceContext("4620816365247727380", IntuitServicesType.QBO, oauthValidator);
            context.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";
            var dataservice = new DataService(context);


            context.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";

            context.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";

            //Find Customer
            QueryService<Customer> customerQueryService = new QueryService<Customer>(context);
            Customer customer = customerQueryService.ExecuteIdsQuery("Select * From Customer StartPosition 1 MaxResults 1").FirstOrDefault();

            //Find Tax Code for Invoice - Searching for a tax code named 'StateSalesTax' in this example
            QueryService<TaxCode> stateTaxCodeQueryService = new QueryService<TaxCode>(context);
            TaxCode stateTaxCode = stateTaxCodeQueryService.ExecuteIdsQuery("Select * From TaxCode Where Name='StateSalesTax' StartPosition 1 MaxResults 1").FirstOrDefault();

            //Find Account - Accounts Receivable account required
            QueryService<Account> accountQueryService = new QueryService<Account>(context);
            Account account = accountQueryService.ExecuteIdsQuery("Select * From Account Where AccountType='Accounts Receivable' StartPosition 1 MaxResults 1").FirstOrDefault();

            //Find Item
            QueryService<Item> itemQueryService = new QueryService<Item>(context);
            Item item = itemQueryService.ExecuteIdsQuery("Select * From Item StartPosition 1 MaxResults 1").FirstOrDefault();

            //Find Term
            QueryService<Term> termQueryService = new QueryService<Term>(context);
            Term term = termQueryService.ExecuteIdsQuery("Select * From Term StartPosition 1 MaxResults 1").FirstOrDefault();


            Invoice invoice = new Invoice();

            //DocNumber - QBO Only, otherwise use DocNumber
            invoice.AutoDocNumber = true;
            invoice.AutoDocNumberSpecified = true;

            //TxnDate
            invoice.TxnDate = DateTime.Now.Date;
            invoice.TxnDateSpecified = true;

            //PrivateNote
            invoice.PrivateNote = "This is a private note";

            //Line
            Line invoiceLine = new Line();
            //Line Description
            invoiceLine.Description = "Invoice line description.";
            //Line Amount
            invoiceLine.Amount = 330m;
            invoiceLine.AmountSpecified = true;
            //Line Detail Type
            invoiceLine.DetailType = LineDetailTypeEnum.SalesItemLineDetail;
            invoiceLine.DetailTypeSpecified = true;
            //Line Sales Item Line Detail
            SalesItemLineDetail lineSalesItemLineDetail = new SalesItemLineDetail();
            //Line Sales Item Line Detail - ItemRef
            lineSalesItemLineDetail.ItemRef = new ReferenceType()
            {
                name = item.Name,
                Value = item.Id
            };
            //Line Sales Item Line Detail - UnitPrice
            lineSalesItemLineDetail.AnyIntuitObject = 33m;
            lineSalesItemLineDetail.ItemElementName = ItemChoiceType.UnitPrice;
            //Line Sales Item Line Detail - Qty
            lineSalesItemLineDetail.Qty = 10;
            lineSalesItemLineDetail.QtySpecified = true;
            //Line Sales Item Line Detail - TaxCodeRef
            //For US companies, this can be 'TAX' or 'NON'
            lineSalesItemLineDetail.TaxCodeRef = new ReferenceType()
            {
                Value = "TAX"
            };
            //Line Sales Item Line Detail - ServiceDate 
            lineSalesItemLineDetail.ServiceDate = DateTime.Now.Date;
            lineSalesItemLineDetail.ServiceDateSpecified = true;
            //Assign Sales Item Line Detail to Line Item
            invoiceLine.AnyIntuitObject = lineSalesItemLineDetail;
            //Assign Line Item to Invoice
            invoice.Line = new Line[] { invoiceLine };

            //TxnTaxDetail
            TxnTaxDetail txnTaxDetail = new TxnTaxDetail();

            Line taxLine = new Line();
            taxLine.DetailType = LineDetailTypeEnum.TaxLineDetail;
            TaxLineDetail taxLineDetail = new TaxLineDetail();
            //Assigning the fist Tax Rate in this Tax Code
            taxLine.AnyIntuitObject = taxLineDetail;
            txnTaxDetail.TaxLine = new Line[] { taxLine };
            invoice.TxnTaxDetail = txnTaxDetail;

            //Customer (Client)
            invoice.CustomerRef = new ReferenceType()
            {
                name = customer.DisplayName,
                Value = customer.Id
            };

            //Billing Address
            PhysicalAddress billAddr = new PhysicalAddress();
            billAddr.Line1 = "123 Main St.";
            billAddr.Line2 = "Unit 506";
            billAddr.City = "Brockton";
            billAddr.CountrySubDivisionCode = "MA";
            billAddr.Country = "United States";
            billAddr.PostalCode = "02301";
            billAddr.Note = "Billing Address Note";
            invoice.BillAddr = billAddr;

            //Shipping Address
            PhysicalAddress shipAddr = new PhysicalAddress();
            shipAddr.Line1 = "100 Fifth Ave.";
            shipAddr.City = "Waltham";
            shipAddr.CountrySubDivisionCode = "MA";
            shipAddr.Country = "United States";
            shipAddr.PostalCode = "02452";
            shipAddr.Note = "Shipping Address Note";
            invoice.ShipAddr = shipAddr;

            //SalesTermRef
            invoice.SalesTermRef = new ReferenceType()
            {
                name = term.Name,
                Value = term.Id
            };

            //DueDate
            invoice.DueDate = DateTime.Now.AddDays(30).Date;
            invoice.DueDateSpecified = true;

            //ARAccountRef
            invoice.ARAccountRef = new ReferenceType()
            {
                name = account.Name,
                Value = account.Id
            };

            Invoice invoiceAdded = dataservice.Add(invoice);
            Console.WriteLine(invoiceAdded);
        }

        public ActionResult LoadClientsData()
        {
            try
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                int recordsTotal = 0;
                var context=GetServiceContext();
                QueryService<Customer> customerQueryService = new QueryService<Customer>(context);
                List<Customer> customerList = customerQueryService.ExecuteIdsQuery("Select * From Customer ").Where(m => (m.CompanyName != null)).ToList();
                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerList = customerList.Where(m => (m.CompanyName.ToLower().Contains(searchValue))).ToList().Skip(skip).Take(pageSize).ToList();
                }
                return Json(new { draw = draw, recordsFiltered = customerList.Count(), recordsTotal = customerList.Count(), data = customerList });
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}