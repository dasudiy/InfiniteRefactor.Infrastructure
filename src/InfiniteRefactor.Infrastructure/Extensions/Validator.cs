using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AirMaster.Infrastructure.Extensions
{
    public static class Validator
    {
        public static bool IsNull(this object o) { return o == null; }
        public static bool IsOddInt(this int o) { return o / 2 * 2 != o; }
        public static bool IsSByte(this object o) { if (o == null) { return false; } sbyte i = 0; return sbyte.TryParse(o.ToString(), out i); }
        public static bool IsByte(this object o) { if (o == null) { return false; } byte i = 0; return byte.TryParse(o.ToString(), out i); }
        public static bool IsShort(this object o) { if (o == null) { return false; } short i = 0; return short.TryParse(o.ToString(), out i); }
        public static bool IsUShort(this object o) { if (o == null) { return false; } ushort i = 0; return ushort.TryParse(o.ToString(), out i); }
        public static bool IsInt(this object o) { if (o == null) { return false; } var i = 0; return int.TryParse(o.ToString(), out i); }
        public static bool IsUInt(this object o) { if (o == null) { return false; } uint i = 0; return uint.TryParse(o.ToString(), out i); }
        public static bool IsDouble(this object o) { if (o == null) { return false; } double i = 0; return double.TryParse(o.ToString(), out i); }
        public static bool IsULong(this object o) { if (o == null) { return false; } ulong i = 0; return ulong.TryParse(o.ToString(), out i); }
        public static bool IsFloat(this object o) { if (o == null) { return false; } float i = 0; return float.TryParse(o.ToString(), out i); }
        public static bool IsLong(this object o) { if (o == null) { return false; } long i = 0; return long.TryParse(o.ToString(), out i); }
        public static bool IsDecimal(this object o) { if (o == null) { return false; } decimal i = 0; return decimal.TryParse(o.ToString(), out i); }
        public static bool IsBool(this object o) { if (o == null) { return false; } bool i = false; return bool.TryParse(o.ToString(), out i); }
        public static bool IsGuid(this object o) { if (o == null) { return false; } Guid i = Guid.Empty; return Guid.TryParse(o.ToString(), out i); }
        public static bool IsDateTime(this object o) { if (o == null) { return false; } DateTime i = DateTime.MinValue; return DateTime.TryParse(o.ToString(), out i); }
        public static bool IsPastDateTime(this object o)
        {
            if (o == null) { return false; }
            DateTime i = DateTime.MinValue;
            if (!DateTime.TryParse(o.ToString(), out i)) { return false; }
            return i > DateTime.MinValue && i < DateTime.Now;
        }
        public static bool IsUserName(this object o) { return o != null && new Regex(@"^[a-zA-Z]\w+$").IsMatch(o.ToString()); }
        public static bool IsPostcode(this object o) { return o != null && new Regex(@"^[1-9]\d{5}$").IsMatch(o.ToString()); }
        public static bool IsAge(this object o) { if (!o.IsInt()) { return false; } var a = int.Parse(o.ToString()); return a >= 18 && a < 200; }
        public static bool IsNaturalNumber(this object o) { if (o == null) { return false; } int i = 0; if (!int.TryParse(o.ToString(), out i)) { return false; } return i >= 0; }
        public static bool IsEmpty(this Guid o) { return Guid.Empty.Equals(o); }
        public static bool IsEmptyString(this object o) { return o == null || string.IsNullOrWhiteSpace(o.ToString()); }

        public static void NotEmpty(this string input, string errorInfo = "不能为空")
        {
            if (!NotEmpty(input)) { throw new Exception(errorInfo); }
        }

        public static bool NotEmpty(this string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        public static void IsEmail(this string email, string errorInfo = "邮件格式不正确")
        {
            if (!IsEmail(email)) { throw new Exception(errorInfo); }
        }

        public static bool IsEmail(this string email)
        {
            return IsMatch(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", email);
        }

        public static bool IsIdentityNumber(this string Id)
        {
            if (!IsMatch(@"^\d{17}[\dxX]$", Id)) { return false; }
            long n = 0;

            if (long.TryParse(Id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }

            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";

            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }

            string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");

            DateTime time = new DateTime();

            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }

            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');

            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');

            char[] Ai = Id.Remove(17).ToCharArray();

            int sum = 0;

            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }

            int y = -1;

            Math.DivRem(sum, 11, out y);

            if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }

            return true;//符合GB11643-1999标准
        }

        public static int GetAge(DateTime dayOfBirth, DateTime? checkDate)
        {
            DateTime today = (checkDate ?? DateTime.Now).Date;
            int age = today.Year - dayOfBirth.Year;
            if (dayOfBirth > today.AddYears(-age)) age--;
            return age;
        }

        public static bool IsPhoneNumber(this string phone)
        {
            return IsMobilePhoneNumber(phone) || IsLineTelephoneNumber(phone);
        }

        public static bool IsMobilePhoneNumber(this string phone)
        {
            return IsMatch(@"^1\d{10}$", phone);
        }

        public static bool IsLineTelephoneNumber(this string phone)
        {
            return IsMatch(@"^0\d{10,11}$", phone);
        }

        public static bool IsPostcode(this string postcode)
        {
            return IsMatch(@"^\d{6}$", postcode);
        }

        public static bool IsMatch(this string regex, string input)
        {
            return Regex.IsMatch(input, regex);
        }

        public static void IsMatch(this string regex, string input, string errorInfo = "输入格式不正确")
        {
            if (!IsMatch(regex, input))
            {
                throw new Exception(errorInfo);
            }
        }

        public static bool IsAllGB2312ChineseChars(this string input)
        {
            var encoding = System.Text.Encoding.GetEncoding("gbk");
            var result = true;
            foreach (var c in input)
            {
                var bytes = encoding.GetBytes(new char[] { c });
                if (bytes.Length < 2)
                {//跳过半角字符...
                    continue;
                }

                result &= bytes[0] >= 176 && bytes[0] <= 247 && bytes[1] >= 160 && bytes[1] <= 254;

                if (!result) { break; }
            }

            return result;
        }

        public static bool IsAllGBKChineseChars(this string input)
        {
            var encoding = System.Text.Encoding.GetEncoding("gbk");
            var result = true;
            foreach (var c in input)
            {
                var bytes = encoding.GetBytes(new char[] { c });
                if (bytes.Length < 2)
                {//跳过半角字符...
                    continue;
                }

                result &= bytes[0] >= 129 && bytes[0] <= 254 && bytes[1] >= 64 && bytes[1] <= 254;

                if (!result) { break; }
            }

            return result;
        }

        /// <summary>
        /// 检查密码是否符合要求：
        /// 1. 长度至少8位
        /// 2. 包含字母
        /// 3. 包含数字
        /// 4. 包含特殊字符
        /// </summary>
        /// <param name="password">要验证的密码</param>
        /// <returns>是否符合要求</returns>
        public static bool IsValidPassword(this string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // 检查长度
            if (password.Length < 8)
                return false;

            // 使用正则表达式检查是否包含字母、数字、特殊字符
            var hasLetter = new Regex(@"[a-zA-Z]");
            var hasDigit = new Regex(@"\d");
            var hasSpecialChar = new Regex(@"[!@#$%^&*(),.?""':{}|<>]"); // 可以根据需求扩展特殊字符集合

            if (!hasLetter.IsMatch(password))
                return false;

            if (!hasDigit.IsMatch(password))
                return false;

            if (!hasSpecialChar.IsMatch(password))
                return false;

            // 所有条件都满足
            return true;
        }

        public static bool IsIPv4(this string input)
        {
            if (input == null) return false;
            var arr = input.Split('.');
            if (arr.Length != 4) return false;
            return arr.All(c =>
            {
                if (int.TryParse(c, out int result) == false) { return false; }
                if (result < 0 || result > 255) return false;
                return true;
            });
        }
    }

    /// <summary>
    /// 中国车牌号验证工具
    /// </summary>
    public static class LicensePlateValidator
    {
        // 所有省市区简称（用于匹配第一个汉字）
        private static readonly string[] ChineseProvinces = {
        "京", "津", "冀", "晋", "蒙", "辽", "吉", "黑", "沪", "苏", "浙", "皖", "闽", "赣", "鲁",
        "豫", "鄂", "湘", "粤", "桂", "琼", "渝", "川", "贵", "云", "藏", "陕", "甘", "青", "宁", "新"
    };

        private static readonly Regex PlateRegex = new Regex(
            @"^(" +
            // 普通燃油车 & 新能源（小车）
            $"([{string.Join("", ChineseProvinces)}])[\\u4e00-\\u9fa5·\\s-]?[A-Z]([A-Z0-9]{{4,5}})[A-Z0-9警领使]?$|" +

            // 新能源大型车（6位数字/字母 + 1字母）
            $"([{string.Join("", ChineseProvinces)}])[\\u4e00-\\u9fa5·\\s-]?[A-Z]([A-Z0-9]{{6}})[DF]$|" +

            // 新能源小型车（5位数字 + 1字母 D/F）
            $"([{string.Join("", ChineseProvinces)}])[\\u4e00-\\u9fa5·\\s-]?[A-Z]([A-Z0-9]{{5}})[DF]$|" +

            // 武警车辆 WJ
            @"WJ[京津沪渝冀晋蒙辽吉黑苏浙皖闽赣鲁豫鄂湘粤桂琼川贵云藏陕甘青宁新]?[A-Z]([A-Z0-9]{{4,5}})$|" +
            @"WJ[京津沪渝冀晋蒙辽吉黑苏浙皖闽赣鲁豫鄂湘粤桂琼川贵云藏陕甘青宁新]?([A-Z0-9]{{2,4}})[A-Z]$|" +

            // 军车（如 甲A1234）
            @"[甲乙丙丁戊己庚辛壬癸][A-Z]([A-Z0-9]{{4}})$|" +

            // 使馆/领事馆
            $"([{string.Join("", ChineseProvinces)}])[A-Z]([A-Z0-9]{{4}})(使|领)$|" +

            // 临时牌照
            @"(临|临牌|天临)[京津沪渝冀晋蒙辽吉黑苏浙皖闽赣鲁豫鄂湘粤桂琼川贵云藏陕甘青宁新]" +
            @"[A-Z]([A-Z0-9]{{5,6}})$" +
            ")$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 验证车牌号是否合法
        /// </summary>
        /// <param name="plate">车牌号字符串</param>
        /// <returns>是否合法</returns>
        public static bool IsLicensePlate(this string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return false;

            // 清理常见分隔符（空格、点、横线）
            var clean = Regex.Replace(plate.Trim(), @"[·\.\s-]", "");

            // 最短4位，最长9位（含省份）
            if (clean.Length < 4 || clean.Length > 9)
                return false;

            return PlateRegex.IsMatch(clean);
        }

        /// <summary>
        /// 获取车牌类型
        /// </summary>
        /// <param name="plate">车牌号</param>
        /// <returns>类型描述</returns>
        public static string GetPlateType(this string plate)
        {
            if (!IsLicensePlate(plate))
                return "无效车牌";

            var clean = Regex.Replace(plate.Trim(), @"[·\.\s-]", "");

            if (clean.StartsWith("WJ"))
                return "武警车辆";
            if (clean[0] >= '甲' && clean[0] <= '癸')
                return "军车";
            if (clean.EndsWith("使") || clean.EndsWith("领"))
                return "使馆/领事馆车辆";
            if (clean.StartsWith("临") || clean.StartsWith("天临"))
                return "临时牌照";

            // 新能源判断
            if (clean.Length == 8 && "DF".Contains(clean[^1]))
                return "新能源大型汽车";
            if (clean.Length == 7 && "DF".Contains(clean[^1]))
                return "新能源小型汽车";

            return "普通燃油车";
        }
    }

    public static class FileTypeValidator
    {
        // 定义文件类型及其对应的 Magic Number（支持多个）
        private static readonly Dictionary<string, List<byte[]>> KnownSignatures = new()
        {
            // 图像
            { ".jpg", new() { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new() { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new() { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".gif", new() { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".bmp", new() { new byte[] { 0x42, 0x4D } } }, // "BM"

            // 文档与压缩包
            { ".pdf", new() { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // "%PDF"
            { ".doc", new() { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } }, // OLE2
            { ".docx", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".xlsx", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".pptx", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".zip", new()
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 }
                }
            },
            { ".rar", new()
                {
                    new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 }, // RAR v1.5 - v4
                    new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 } // RAR v5+
                }
            },

            // 视频
            //{ ".mp4", new() { /* 特殊处理 */ } }, // 单独逻辑

            // 可执行文件
            { ".exe", new() { new byte[] { 0x4D, 0x5A } } }, // "MZ"
            { ".dll", new() { new byte[] { 0x4D, 0x5A } } }
        };

        public static bool IsMp4File(byte[] header)
        {
            // MP4 文件以一系列 "boxes" 开始，第一个通常是 ftyp box
            // ftyp 的 ASCII 是 66 74 79 70
            // 我们在前 12 字节内查找 "ftyp"
            if (header.Length < 8) return false;
            int maxIndex = Math.Min(12, header.Length - 3);
            for (int i = 4; i < maxIndex; i++)
            {
                if (header[i] == 0x66 && // 'f'
                    header[i + 1] == 0x74 && // 't'
                    header[i + 2] == 0x79 && // 'y'
                    header[i + 3] == 0x70)   // 'p'
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsFileTypeValid(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
                return false;

            if (!KnownSignatures.ContainsKey(extension))
                return false; // 不支持的扩展名，可按需处理
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] header = new byte[32]; // 足够读取最长签名
            int bytesRead = fs.Read(header, 0, header.Length);
            if (bytesRead == 0) return false;
            return IsFileTypeValid(extension, header);
        }
        public static bool IsFileTypeValid(string extension, byte[] header)
        {
            if (header == null || header.Length == 0) return false;
            try
            {
                if (extension == ".mp4") { return IsMp4File(header); }
                if (!KnownSignatures.TryGetValue(extension, out var signatures))
                    return false; // 不支持的扩展名，可按需处理
                // 检查是否匹配任意一个签名
                foreach (var sig in signatures)
                {
                    if (sig.Length > header.Length) continue;
                    bool match = true;
                    for (int i = 0; i < sig.Length; i++)
                    {
                        if (header[i] != sig[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match) return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static IEnumerable<string> GetActualFileExtensions(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] header = new byte[32];
            int bytesRead = fs.Read(header, 0, header.Length);
            if (bytesRead == 0) return Enumerable.Empty<string>();
            return GetActualFileExtensions(header);
        }
        public static IEnumerable<string> GetActualFileExtensions(byte[] header)
        {
            foreach (var kvp in KnownSignatures)
            {
                string ext = kvp.Key;
                if (ext == ".mp4")
                {
                    if (IsMp4File(header))
                    {
                        yield return ext;
                    }
                    continue;
                }
                foreach (var sig in kvp.Value)
                {
                    if (sig.Length > header.Length) continue;
                    bool match = true;
                    for (int i = 0; i < sig.Length; i++)
                    {
                        if (header[i] != sig[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        yield return ext;
                    }
                }
            }
        }
    }
}
