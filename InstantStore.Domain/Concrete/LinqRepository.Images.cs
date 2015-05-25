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
                    SmallThumbnail = CreateThumbnail(image.Image1, 64, 64, true),
                    LargeThumbnail = CreateThumbnail(image.Image1, 235, 200, false)
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

        private Binary CreateThumbnail(Binary binaryImage, int width, int height, bool advancedCrop = false)
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

                        float srcImageRatio = (float)image.Width / (float)image.Height;
                        float dstImageRatio = (float)width / (float)height;

                        SD.Rectangle srcRect = new SD.Rectangle(0, 0, image.Width, image.Height);
                        SD.Rectangle dstRect = new SD.Rectangle(0, 0, width, height);

                        bool useWidthConst =
                            (srcImageRatio > 1.0f && srcImageRatio > dstImageRatio) ||
                            (srcImageRatio > 1.0f && srcImageRatio > dstImageRatio);
                        bool useHeightConst =
                            (srcImageRatio < 1.0f && dstImageRatio > srcImageRatio) ||
                            (srcImageRatio < 1.0f && dstImageRatio > srcImageRatio);

                        // Crop 
                        if (!advancedCrop)
                        {
                            if (useWidthConst)
                            {
                                int newSrcHeight = (int)(width / srcImageRatio);
                                int srcTopOffset = (height - newSrcHeight) / 2;
                                dstRect = new SD.Rectangle(0, srcTopOffset, width, newSrcHeight);
                            }
                            else if (useHeightConst)
                            {
                                int newSrcWidth = (int)(height * srcImageRatio);
                                int srcLeftOffset = (width - newSrcWidth) / 2;
                                dstRect = new SD.Rectangle(srcLeftOffset, 0, newSrcWidth, height);
                            }
                        }
                        else
                        {
                            if (useWidthConst)
                            {
                                int newSrcWidth = (int)(image.Height * dstImageRatio);
                                int srcLeftOffset = (image.Width - newSrcWidth) / 2;
                                srcRect = new SD.Rectangle(srcLeftOffset, 0, newSrcWidth, image.Height);
                            }
                            else if (useHeightConst)
                            {
                                int newSrcHeight = (int)(image.Width / dstImageRatio);
                                int srcTopOffset = (image.Height - newSrcHeight) / 2;
                                srcRect = new SD.Rectangle(0, srcTopOffset, image.Width, newSrcHeight);
                            }
                        }

                        Graphic.DrawImage(image, dstRect, srcRect, SD.GraphicsUnit.Pixel);
                        MemoryStream ms = new MemoryStream();
                        thumbnail.Save(ms, SD.Imaging.ImageFormat.Png);
                        return new Binary(ms.GetBuffer());
                    }
                }
            }
        }
    }
}
