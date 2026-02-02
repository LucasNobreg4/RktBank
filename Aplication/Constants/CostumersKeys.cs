using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Constants
{
    public static class CostumersKeys
    {
        public const string All = "customers:all";
        public const string ByIdPrefix = "customers:id:";
        public const string ByDocumentPrefix = "customers:doc:";
        public const string Initialized = "customers:initialized";

        public static string ById(string id) => $"{ByIdPrefix}{id}";
        public static string ByDocument(string document) => $"{ByDocumentPrefix}{document}";
    }
}
