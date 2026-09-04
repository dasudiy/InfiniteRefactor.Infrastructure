using System;
using System.Text;
using System.Text.RegularExpressions;
using InfiniteRefactor.Infrastructure.Extensions;

namespace InfiniteRefactor.Infrastructure.Utilities
{
    public static class AirHelper
    {
        public static bool IsPN(string resultStr)
        {
            try
            {
                Regex regPN = new Regex(@"\s{2}(\+)");
                var pnflag = regPN.Matches(resultStr);
                return pnflag.Count != 0;
            }
            catch
            {
                return false;
            }
        }

        public static string ParseTicketNo(string ticketNo)
        {
            if (ticketNo != null)
            {
                ticketNo = ticketNo.Trim();
                if (Regex.IsMatch(ticketNo, @"\d{13}"))
                {
                    return string.Format("{0}-{1}", ticketNo.Substring(0, 3), ticketNo.Substring(3));
                }
            }

            return ticketNo;
        }

        public static string ParseTicketNoWithoutMinus(string ticketNo)
        {
            return ParseTicketNo(ticketNo).Replace("-", "");
        }

        public static bool IsValidTicketNo(string ticketNo)
        {
            return ticketNo != null && Regex.IsMatch(ticketNo, @"^\d{3}-?\d{10}$");
        }

        public static bool IsValidFlightNo(string flightNo)
        {
            return flightNo != null && Regex.IsMatch(flightNo, @"^\w{2,}$");
        }

        public static string ToPNRDateString(this DateTime date)
        {
            //if (date.Year == DateTime.Now.Year)
            //{
            //    return date.ToString("ddMMM", System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
            //}
            //else
            //{
            return date.ToString("ddMMMyy", System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat).ToUpperInvariant();
            //}
        }

        public static DateTime FromPNRDateString(string date, bool past = false)
        {
            if (date == ".")
            {
                return DateTime.Today;
            }
            if (date == "+")
            {
                return DateTime.Today.AddDays(1);
            }

            DateTime result;
            if (date.Length > 5)
            {
                result = DateTime.ParseExact(date, "ddMMMyy", System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
            }
            else
            {
                result = DateTime.ParseExact(date, "dMMM", System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
                if (result < DateTime.Today && !past)
                {
                    result = result.AddYears(1);
                }
            }
            return result;
        }

        /// <summary>
        /// 移除航班号中的数字0，QR017->QR17
        /// </summary>
        /// <param name="flightNo"></param>
        /// <returns></returns>
        public static string TrimFlightNo(string flightNo)
        {
            return flightNo.Substring(0, 2) + flightNo.Substring(2).TrimStart('0');
        }

        public static bool IsPNR(string pnr)
        {
            return pnr != null && pnr.Length == 6 && Regex.IsMatch(pnr, @"^[a-zA-Z0-9]{6}$");
        }

        public static bool IsValidPNR(string pnr)
        {
            return IsPNR(pnr) && pnr != "ERROR" && pnr != "888888";
        }

        public static string FixPageLayout(string source)
        {
            var sb = new StringBuilder();
            foreach (var row in source.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var currentRow = row;
            NEXT:
                if (currentRow.Length > 80)
                {
                    sb.Append(currentRow.Substring(0, 80));
                    sb.Append("\r");
                    currentRow = currentRow.Substring(80);
                    goto NEXT;
                }
                else
                {
                    sb.Append(currentRow);
                    sb.Append("\r");
                }
            }
            return sb.ToString();
        }

        public static decimal Round(decimal number, int position)
        {
            return Math.Round(number / Math.Pow(10, position).To<decimal>(), MidpointRounding.AwayFromZero) * Math.Pow(10, position).To<decimal>();
        }

        public static double Round(double number, int position)
        {
            return Math.Round(number / Math.Pow(10, position).To<double>(), MidpointRounding.AwayFromZero) * Math.Pow(10, position).To<double>();
        }

        public static decimal Ceiling(decimal number, int position)
        {
            return Math.Ceiling(number / Math.Pow(10, position).To<decimal>()) * Math.Pow(10, position).To<decimal>();
        }

        public static double Ceiling(double number, int position)
        {
            return Math.Ceiling(number / Math.Pow(10, position).To<double>()) * Math.Pow(10, position).To<double>();
        }

        public static decimal Floor(decimal number, int position)
        {
            return Math.Floor(number / Math.Pow(10, position).To<decimal>()) * Math.Pow(10, position).To<decimal>();
        }

        public static double Floor(double number, int position)
        {
            return Math.Floor(number / Math.Pow(10, position).To<double>()) * Math.Pow(10, position).To<double>();
        }

        public static string GetCityCode(string airportCode)
        {
            // https://wikitravel.org/en/Metropolitan_Area_Airport_Codes
            if (airportCode.In("BKK", "DMK")) { return "BKK"; }
            if (airportCode.In("PEK", "NAY", "PKX")) { return "BJS"; }
            if (airportCode.In("KIX", "ITM", "UKB")) { return "OSA"; }
            if (airportCode.In("CTS", "OKD")) { return "SPK"; }
            if (airportCode.In("ICN", "GMP")) { return "SEL"; }
            if (airportCode.In("SHA", "PVG")) { return "SHA"; }
            if (airportCode.In("NRT", "HND")) { return "TYO"; }
            if (airportCode.In("CGK", "HLP")) { return "JKT"; }
            if (airportCode.In("OTP", "BBU")) { return "BUH"; }
            if (airportCode.In("BRU", "CRL")) { return "BRU"; }
            if (airportCode.In("BQH", "LCY", "LGW", "LTN", "LHR", "SEN", "STN")) { return "LON"; }
            if (airportCode.In("BGY", "MXP", "LIN", "PMF")) { return "MIL"; }
            if (airportCode.In("SVO", "DME", "VKO", "BKA")) { return "MOW"; }
            if (airportCode.In("OSL", "TRF", "RYG")) { return "OSL"; }
            if (airportCode.In("CDG", "ORY", "LBG")) { return "PAR"; }
            if (airportCode.In("KEF", "RKV")) { return "REK"; }
            if (airportCode.In("FCO", "CIA")) { return "ROM"; }
            if (airportCode.In("ARN", "NYO", "BMA", "VST")) { return "STO"; }
            if (airportCode.In("TFN", "TFS")) { return "TCI"; }
            if (airportCode.In("ORD", "MDW", "RFD")) { return "CHI"; }
            if (airportCode.In("DAL", "DFW")) { return "DFW"; }
            if (airportCode.In("DTW", "DET", "YIP")) { return "DTT"; }
            if (airportCode.In("YEG")) { return "YEA"; }
            if (airportCode.In("LAX")) { return "LAX"; }
            if (airportCode.In("MIA", "FLL", "PBI")) { return "QMI"; }
            if (airportCode.In("YUL", "YMY")) { return "YMQ"; }
            if (airportCode.In("JFK", "EWR", "LGA", "HPN")) { return "NYC"; }
            if (airportCode.In("YYZ", "YTZ", "YKF")) { return "YTO"; }
            if (airportCode.In("IAD", "DCA", "BWI")) { return "WAS"; }
            if (airportCode.In("CNF", "PLU")) { return "BHZ"; }
            if (airportCode.In("EZE", "AEP")) { return "BUE"; }
            if (airportCode.In("GRU", "CGH", "VCP")) { return "SAO"; }
            if (airportCode.In("IKA", "THR")) { return "THR"; }
            if (airportCode.In("XIY")) { return "SIA"; }
            if (airportCode.In("TFU", "CTU")) { return "CTU"; }
            if (airportCode.In("WHA")) { return "WHU"; }
            if (airportCode.In("DDR")) { return "RKZ"; }
            if (airportCode.In("JNH")) { return "JXS"; }

            return airportCode;
        }

    }
}
