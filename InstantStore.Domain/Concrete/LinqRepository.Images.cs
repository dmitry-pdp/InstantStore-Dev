using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SD = System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public Image GetImageById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Images.FirstOrDefault(x => x.Id == id);
            }
        }

        public Guid AddImage(Image image)
        {
            using (var context = new InstantStoreDataContext())
            {
                image.Id = Guid.NewGuid();
                context.Images.InsertOnSubmit(image);

                context.ImageThumbnails.InsertOnSubmit(new ImageThumbnail
                {
                    Id = image.Id,
                    SmallThumbnail = CreateThumbnail(image.Image1, 64, 64),
                    LargeThumbnail = CreateThumbnail(image.Image1, 320, 240)
                });

                context.SubmitChanges();
                return image.Id;
            }
        }

        public ImageThumbnail GetImageThumbnailById(Guid id)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.ImageThumbnails.FirstOrDefault(x => x.Id == id);
            }
        }

        private Binary CreateThumbnail(Binary binaryImage, int width, int height)
        {
            using (var inputImageStream = new MemoryStream(binaryImage.ToArray()))
            {
                var image = SD.Image.FromStream(inputImageStream);
                using (var thumbnail = new SD.Bitmap(width, height))
                {
                    thumbnail.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    using (SD.Graphics Graphic = SD.Graphics.FromImage(thumbnail))
                    {
                        Graphic.SmoothingMode = SmoothingMode.AntiAlias;
                        Graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        Graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        Graphic.DrawImage(image, new SD.Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, SD.GraphicsUnit.Pixel);
                        MemoryStream ms = new MemoryStream();
                        thumbnail.Save(ms, SD.Imaging.ImageFormat.Png);
                        return new Binary(ms.GetBuffer());
                    }
                }
            }
        }
    }
}
