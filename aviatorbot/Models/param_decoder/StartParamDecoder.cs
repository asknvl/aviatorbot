using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.param_decoder
{
    public static class StartParamDecoder
    {
        public static DecodedParam Decode(string input)
        {
            DecodedParam res = new DecodedParam();

            if (input == null)
                return res;

            input = input.Trim();

            if (input.Length == 8)
            {

                string h_source = input.Substring(0, 1);

                switch (h_source)
                {
                    case "0":
                        res.source = "IND_TG";
                        break;
                    case "1":
                        res.source = "IND_INSTA";
                        break;
                    default:
                        res.source = "UNKNOWN";
                        break;
                }


                string s_num = input.Substring(1, 3);

                try
                {
                    res.num = Convert.ToInt32("0x" + s_num, 16);
                }
                catch (Exception ex)
                {
                    res.num = -1;
                }

                res.buyer = input.Substring(4, 2);
                res.closer = input.Substring(6, 2);
            }

            return res;
        }
    }

    public class DecodedParam
    {
        public string buyer { get; set; } = "UNKNOWN";
        public string closer { get; set; } = "UNKNOWN";
        public string source { get; set; } = "UNKNOWN";
        public int num { get; set; } = -1;
    }
}
