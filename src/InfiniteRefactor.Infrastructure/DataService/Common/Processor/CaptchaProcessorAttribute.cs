//using AirMaster.Infrastructure.DataService.Attributes;
//using AirMaster.Infrastructure.DataService.Processor;
//using AirMaster.Infrastructure.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace AirMaster.Infrastructure.DataService.Implement
//{
//    public class CaptchaRequiredAttribute : PreProcessorAttribute
//    {
//        public string CaptchaArgumentName { get; set; }

//        public CaptchaRequiredAttribute()
//        {
//            CaptchaArgumentName = "captcha";
//        }

//        public override ProcessResult Process(DataServiceRequest request)
//        {
//            var captcha = request.ReadParameter<string>(CaptchaArgumentName, null);
//            var sessionCaptcha = request.Context.Session["captcha"];
//            if (captcha == null)
//            {
//                return new ProcessResult { CancelProcess = true, Message = "必须输入验证码才能执行此操作！", SourceName = "CaptchaRequiredException" };
//            }
//            else if (sessionCaptcha == null)
//            {
//                return new ProcessResult { CancelProcess = true, Message = "验证码已过期，请重新获取！", SourceName = "CaptchaRequiredException" };
//            }
//            else if (!string.Equals(captcha, sessionCaptcha.To<string>(), StringComparison.InvariantCultureIgnoreCase))
//            {
//                return new ProcessResult { CancelProcess = true, Message = "验证码错误！", SourceName = "CaptchaRequiredException" };
//            }
//            else
//            {
//                request.Context.Session.Remove("captcha");
//                return ProcessResult.Default;
//            }
//        }
//    }

//    public class CaptchaGeneratorAttribute : PostProcessorAttribute
//    {
//        public int Height { get; set; }
//        public int Width { get; set; }

//        public CaptchaGeneratorAttribute(int width, int height)
//        {
//            this.Width = width;
//            this.Height = height;
//        }

//        public override Processor.ProcessResult Process(DataServiceResponse response)
//        {
//            if (response.Result != null)
//            {
//                var text = response.Result.ToString();
//                response.Context.Session["captcha"] = text;

//                var img = new RandomImage(text, Width, Height);
//                var format = GetImageFormatName(response.Context.RouteInfo.FormatName);
//                response.ContentType = "image/" + format.ToString().ToLowerInvariant();
//                img.Image.Save(response.OutputStream, format);

//                response.End();
//            }
//            return new ProcessResult { Last = true };
//        }

//        static ImageFormat GetImageFormatName(string name)
//        {
//            try
//            {
//                Guid format;
//                if (_knownImageFormats.TryGetValue(name, out format))
//                {
//                    return new ImageFormat(format);
//                }
//                return ImageFormat.Jpeg;
//            }
//            catch (Exception)
//            {
//                return ImageFormat.Jpeg;
//            }
//        }

//        private static readonly Dictionary<string, Guid> _knownImageFormats =
//            (from p in typeof(ImageFormat).GetProperties(BindingFlags.Static | BindingFlags.Public)
//             where p.PropertyType == typeof(ImageFormat)
//             let value = (ImageFormat)p.GetValue(null, null)
//             select new { Guid = value.Guid, Name = value.ToString() })
//            .ToDictionary(p => p.Name, p => p.Guid);

//        private class RandomImage
//        {
//            //Default Constructor 
//            public RandomImage() { }
//            //property
//            public string Text
//            {
//                get { return this.text; }
//            }
//            public Bitmap Image
//            {
//                get { return this.image; }
//            }
//            public int Width
//            {
//                get { return this.width; }
//            }
//            public int Height
//            {
//                get { return this.height; }
//            }
//            //Private variable
//            private string text;
//            private int width;
//            private int height;
//            private Bitmap image;
//            private Random random = new Random();
//            //Methods declaration
//            public RandomImage(string s, int width, int height)
//            {
//                this.text = s;
//                this.SetDimensions(width, height);
//                this.GenerateImage();
//            }
//            public void Dispose()
//            {
//                GC.SuppressFinalize(this);
//                this.Dispose(true);
//            }
//            protected virtual void Dispose(bool disposing)
//            {
//                if (disposing)
//                    this.image.Dispose();
//            }
//            private void SetDimensions(int width, int height)
//            {
//                if (width <= 0)
//                    throw new ArgumentOutOfRangeException("width", width,
//                        "Argument out of range, must be greater than zero.");
//                if (height <= 0)
//                    throw new ArgumentOutOfRangeException("height", height,
//                        "Argument out of range, must be greater than zero.");
//                this.width = width;
//                this.height = height;
//            }
//            private void GenerateImage()
//            {
//                Bitmap bitmap = new Bitmap
//                  (this.width, this.height, PixelFormat.Format32bppArgb);
//                Graphics g = Graphics.FromImage(bitmap);
//                g.SmoothingMode = SmoothingMode.AntiAlias;
//                Rectangle rect = new Rectangle(0, 0, this.width, this.height);
//                HatchBrush hatchBrush = new HatchBrush(HatchStyle.SmallConfetti,
//                    Color.LightGray, Color.White);
//                g.FillRectangle(hatchBrush, rect);
//                SizeF size;
//                float fontSize = rect.Height + 1;
//                Font font;

//                do
//                {
//                    fontSize--;
//                    font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
//                    size = g.MeasureString(this.text, font);
//                } while (size.Width > rect.Width);
//                StringFormat format = new StringFormat();
//                format.Alignment = StringAlignment.Center;
//                format.LineAlignment = StringAlignment.Center;
//                GraphicsPath path = new GraphicsPath();
//                //path.AddString(this.text, font.FontFamily, (int) font.Style, 
//                //    font.Size, rect, format);
//                path.AddString(this.text, font.FontFamily, (int)font.Style, 50, rect, format);
//                float v = 4F;
//                PointF[] points =
//          {
//                new PointF(this.random.Next(rect.Width) / v, this.random.Next(
//                   rect.Height) / v),
//                new PointF(rect.Width - this.random.Next(rect.Width) / v, 
//                    this.random.Next(rect.Height) / v),
//                new PointF(this.random.Next(rect.Width) / v, 
//                    rect.Height - this.random.Next(rect.Height) / v),
//                new PointF(rect.Width - this.random.Next(rect.Width) / v,
//                    rect.Height - this.random.Next(rect.Height) / v)
//          };
//                Matrix matrix = new Matrix();
//                matrix.Translate(0F, 0F);
//                path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);
//                hatchBrush = new HatchBrush(HatchStyle.Percent10, Color.Black, Color.SkyBlue);
//                g.FillPath(hatchBrush, path);
//                int m = Math.Max(rect.Width, rect.Height);
//                for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
//                {
//                    int x = this.random.Next(rect.Width);
//                    int y = this.random.Next(rect.Height);
//                    int w = this.random.Next(m / 50);
//                    int h = this.random.Next(m / 50);
//                    g.FillEllipse(hatchBrush, x, y, w, h);
//                }
//                font.Dispose();
//                hatchBrush.Dispose();
//                g.Dispose();
//                this.image = bitmap;
//            }
//        }
//    }
//}
