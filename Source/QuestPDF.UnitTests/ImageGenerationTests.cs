﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace QuestPDF.UnitTests
{
    public class GenerateImageTests
    {
        [Test]
        [TestCaseSource(nameof(GeneratedImageResolutionCorrespondsToTargetDpi_TestCases))]
        public void GeneratedImageResolutionCorrespondsToTargetDpi(GeneratedImageResolutionCorrespondsToTargetDpi_TestCaseItem testCase)
        {
            // act
            var images = Document
                .Create(document => document.Page(page =>
                {
                    page.Size(testCase.PageSize);
                    page.Content().Text("Test");
                }))
                .GenerateImages(new ImageGenerationSettings { RasterDpi = testCase.TargetDpi })
                .ToList();
            
            // assert
            Assert.That(images, Has.Exactly(1).Items);

            var imageData = images.First();
            Assert.That(imageData, Is.Not.Null);
            
            using var image = SKImage.FromEncodedData(imageData);
            Assert.That(image, Is.Not.Null);
            
            Assert.That(image.Width, Is.EqualTo(testCase.ExpectedImageSize.Width));
            Assert.That(image.Height, Is.EqualTo(testCase.ExpectedImageSize.Height));
        }

        public record GeneratedImageResolutionCorrespondsToTargetDpi_TestCaseItem(PageSize PageSize, int TargetDpi, ImageSize ExpectedImageSize);
        
        public static GeneratedImageResolutionCorrespondsToTargetDpi_TestCaseItem[] GeneratedImageResolutionCorrespondsToTargetDpi_TestCases =
        {
            new(new PageSize(150, 250), 72, new ImageSize(150, 250)),
            new(new PageSize(200, 300), 144, new ImageSize(400, 600)),
            new(new PageSize(250, 350), 360, new ImageSize(1250, 1750)),
        };



        [Test]
        public void GeneratedImageSizeCorrespondsToImageQuality()
        {
            // arrange
            var document = Document.Create(document => document.Page(page =>
            {
                page.Content().Image("Resources/photo.jpg");
            }));
            
            // act
            var imageSizeWithLowQuality = CheckImageSize(ImageCompressionQuality.Low);
            var imageSizeWithMediumQuality = CheckImageSize(ImageCompressionQuality.Medium);
            var imageSizeWithHighQuality = CheckImageSize(ImageCompressionQuality.High);
            
            // assert
            Assert.That(imageSizeWithLowQuality, Is.LessThan(imageSizeWithMediumQuality));
            Assert.That(imageSizeWithMediumQuality, Is.LessThan(imageSizeWithHighQuality));

            int CheckImageSize(ImageCompressionQuality quality)
            {
                var images = document
                    .GenerateImages(new ImageGenerationSettings() { ImageFormat = ImageFormat.Jpeg, ImageCompressionQuality = quality })
                    .ToList();
                
                Assert.That(images, Has.Exactly(1).Items);

                var image = images.First();
                Assert.That(image, Is.Not.Null);

                return image.Length;
            }
        }
        
        
        [TestCase(ImageFormat.Png)]
        [TestCase(ImageFormat.Jpeg)]
        [TestCase(ImageFormat.Webp)]
        public void ImageFormatIsRespected(ImageFormat imageFormat)
        {
            var images = Document
                .Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Content().Padding(25).AspectRatio(2).Background(Colors.Red.Medium);
                    });
                })
                .GenerateImages(new ImageGenerationSettings() { ImageFormat = imageFormat })
                .ToList();
            
            Assert.That(images, Has.Exactly(1).Items);

            var imageData = images.First();
            Assert.That(imageData, Is.Not.Null);

            using var imageStream = new MemoryStream(imageData);
            using var imageCodec = SKCodec.Create(imageStream);

            Assert.That(imageCodec, Is.Not.Null);
            Assert.That(imageCodec.EncodedFormat.ToString(), Is.EqualTo(imageFormat.ToString()));
        }
    }
}