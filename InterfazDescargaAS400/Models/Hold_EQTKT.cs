using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfazDescargaAS400.Models
{
    public class Hold_EQTKT
    {
        public String CD_BRANCH { get; set; }
        public String ACCOUNT { get; set; }
        public String SUFIX { get; set; }
        public int HOLD_NO { get; set; }
        public String START_DATE { get; set; }
        public String EXPIRY_DATE { get; set; }
        public decimal AMOUNT { get; set; }
        public String RESP_CODE { get; set; }
        public String REASON_CODE { get; set; }
        public String DSC_LINE1 { get; set; }
        public String DSC_LINE2 { get; set; }
        public String DSC_LINE3 { get; set; }
        public String DSC_LINE4 { get; set; }
        public String INPUT_DATE { get; set; }

    }
}
