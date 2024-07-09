﻿using JewelryProduction.DbContext;
using JewelryProduction.DTO;
using JewelryProduction.Interface;
using JewelryProduction.Common;
using Microsoft.EntityFrameworkCore;
using JewelryProduction.Repositories;

namespace JewelryProduction.Services
{
    public class ProductSampleService: IProductSampleService
    {
        private readonly JewelryProductionContext _context;
        private readonly IProductSampleRepository _productSampleRepository;

        public ProductSampleService(JewelryProductionContext context, IProductSampleRepository repository)
        {
            _context = context;
            _productSampleRepository = repository;
        }
        public async Task<List<ProductSampleDTO>> GetRecommendedSamples(string? type, string? style, double? size, string? goldType, List<string>? gemstoneName)
        {
            var allSamples = await _context.ProductSamples
             .Include(ps => ps.Gemstones)
             .Include(ps => ps._3ddesigns)
             .Include(ps => ps.Gold)
             .ToListAsync();

            var recommendedSamples = allSamples
                .Select(sample => new 
                {
                    Sample = sample,
                    Similarity = CalculateSimilarity(type, style, size, goldType, gemstoneName, sample)
                })
                .OrderByDescending(s => s.Similarity)
                .Take(5) // For example, get top 5 recommendations
                .Select(s => new ProductSampleDTO
                {
                    ProductSampleId = s.Sample.ProductSampleId,
                    ProductName = s.Sample.ProductName,
                    Description = s.Sample.Description,
                    Type = s.Sample.Type,
                    Style = s.Sample.Style,
                    Size = s.Sample.Size,
                    Price = s.Sample.Price,
                    GoldType = s.Sample.Gold.GoldType,
                    Image = s.Sample._3ddesigns.FirstOrDefault()?.Image // Take the first image URL
                })
                .ToList();

            return recommendedSamples;
        }
        public async Task<PrefillDTO> PrefillCustomizeRequestAsync(string productSampleId)
        {
            var productSample = await _productSampleRepository.GetProductSampleByIdAsync(productSampleId);

            if (productSample == null)
            {
                throw new KeyNotFoundException($"Product sample with ID {productSampleId} was not found.");
            }

            var primaryGemstone = productSample.Gemstones
                .Where(g => g.CaratWeight > 0.3)
                .Select(g => new AddGemstoneDTO
                {
                    Name = g.Name,
                    Clarity = g.Clarity,
                    Color = g.Color,
                    Shape = g.Shape,
                    Size = g.Size,
                    Cut = g.Cut,
                    CaratWeight = g.CaratWeight,
                }).FirstOrDefault();

            var additionalGemstones = productSample.Gemstones
                .Where(g => g.CaratWeight <= 0.3)
                .Select(g => new Gemstone
                {
                    Name = g.Name,
                    Clarity = g.Clarity,
                    Color = g.Color,
                    Shape = g.Shape,
                    Size = g.Size,
                    Cut = g.Cut,
                    CaratWeight = g.CaratWeight,
                }).ToList();

            return new PrefillDTO
            {
                Type = productSample.Type,
                Style = productSample.Style,
                Quantity = 1, // Default quantity
                PrimaryGemstone = primaryGemstone,
                AdditionalGemstone = additionalGemstones.Select(g => g.Name).ToList(),
                GoldType = productSample.Gold.GoldType,
            };
        }
            public double CalculateSimilarity(string? type, string? style, double? size, string? goldType, List<string>? gemstoneName , ProductSample sample2)
        {
            var gemstones =  _context.Gemstones
            .Where(g => gemstoneName.Contains(g.Name))
            .ToListAsync();

            double similarity = 0;

            if (sample2.Type is not null && type == sample2.Type) similarity += 1;
            if (sample2.Style is not null && style == sample2.Style) similarity += 1;
            if (sample2.Size is not null && size == sample2.Size) similarity += 1;
            if (sample2.Gold is not null && goldType == sample2.Gold.GoldType)
            {
                similarity += 1;
            }
            if (gemstones == sample2.Gemstones) similarity += 1;
            if (gemstoneName != null && gemstoneName.Any())
            {
                var sample2GemstoneNames = sample2.Gemstones.Select(g => g.Name).ToList();
                foreach (var gemstonename in gemstoneName)
                {
                    if (sample2GemstoneNames.Contains(gemstonename))
                    {
                        similarity += 1;
                        break;
                    }
                }
            }

            return similarity;
        }
    }
}
