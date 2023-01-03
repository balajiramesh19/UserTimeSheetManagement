using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTimeSheetManagement.Models;

namespace WebTimeSheetManagement.Interface
{
    public interface IDocument
    {
        int AddDocument(Documents Documents);
        Documents GetDocumentByExpenseID(int? ExpenseID, int? DocumentID);
        List<DocumentsVM> GetListofDocumentByExpenseID(int? ExpenseID);
        List<Documents> GetDocumentsForUserWithinDates(DateTime? FromDate, DateTime? ToDate);
    }

}
