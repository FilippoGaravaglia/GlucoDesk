using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for desserts.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetDessertFoods()
    {
        return
        [
            new(
                "gelato-al-tartufo",
                CarbGuideCategoryId.Desserts,
                10,
                new(
                    "Gelato al tartufo",
                    "Chocolate truffle ice cream"),
                new[]
                {
                    new CarbGuidePortion(115, 39),
                }),
            new(
                "creme-caramel",
                CarbGuideCategoryId.Desserts,
                20,
                new(
                    "Crème caramel",
                    "Crème caramel"),
                new[]
                {
                    new CarbGuidePortion(120, 19),
                }),
            new(
                "crema-nocciole-cacao",
                CarbGuideCategoryId.Desserts,
                30,
                new(
                    "Crema di nocciole e cacao",
                    "Hazelnut and cocoa spread"),
                new[]
                {
                    new CarbGuidePortion(30, 15),
                }),
            new(
                "cioccolato-al-latte",
                CarbGuideCategoryId.Desserts,
                40,
                new(
                    "Cioccolato al latte",
                    "Milk chocolate"),
                new[]
                {
                    new CarbGuidePortion(30, 15),
                }),
            new(
                "strudel-di-mele",
                CarbGuideCategoryId.Desserts,
                50,
                new(
                    "Strudel di mele",
                    "Apple strudel"),
                new[]
                {
                    new CarbGuidePortion(50, 19),
                    new CarbGuidePortion(100, 39),
                    new CarbGuidePortion(150, 58),
                }),
            new(
                "tiramisu",
                CarbGuideCategoryId.Desserts,
                60,
                new(
                    "Tiramisù",
                    "Tiramisu"),
                new[]
                {
                    new CarbGuidePortion(50, 26),
                    new CarbGuidePortion(100, 52),
                    new CarbGuidePortion(150, 78),
                }),
            new(
                "torta-sacher",
                CarbGuideCategoryId.Desserts,
                70,
                new(
                    "Torta Sacher",
                    "Sacher cake"),
                new[]
                {
                    new CarbGuidePortion(50, 34),
                    new CarbGuidePortion(100, 68),
                    new CarbGuidePortion(150, 102),
                }),
            new(
                "torta-saint-honore",
                CarbGuideCategoryId.Desserts,
                80,
                new(
                    "Torta Saint Honoré",
                    "Saint Honoré cake"),
                new[]
                {
                    new CarbGuidePortion(50, 18),
                    new CarbGuidePortion(100, 36),
                    new CarbGuidePortion(150, 54),
                }),
            new(
                "crostata-marmellata",
                CarbGuideCategoryId.Desserts,
                90,
                new(
                    "Crostata di marmellata",
                    "Jam tart"),
                new[]
                {
                    new CarbGuidePortion(50, 36),
                    new CarbGuidePortion(100, 72),
                    new CarbGuidePortion(150, 108),
                }),
            new(
                "biscotti-secchi",
                CarbGuideCategoryId.Desserts,
                100,
                new(
                    "Biscotti secchi",
                    "Plain biscuits"),
                new[]
                {
                    new CarbGuidePortion(15, 12),
                    new CarbGuidePortion(30, 24),
                    new CarbGuidePortion(45, 36),
                }),
            new(
                "biscotti-integrali",
                CarbGuideCategoryId.Desserts,
                110,
                new(
                    "Biscotti tipo integrale",
                    "Wholemeal biscuits"),
                new[]
                {
                    new CarbGuidePortion(15, 11),
                    new CarbGuidePortion(30, 22),
                    new CarbGuidePortion(45, 33),
                }),
            new(
                "merendina-farcita",
                CarbGuideCategoryId.Desserts,
                120,
                new(
                    "Merendina farcita",
                    "Filled snack cake"),
                new[]
                {
                    new CarbGuidePortion(30, 20),
                }),
            new(
                "merendina-cioccolato",
                CarbGuideCategoryId.Desserts,
                130,
                new(
                    "Merendina al cioccolato",
                    "Chocolate snack cake"),
                new[]
                {
                    new CarbGuidePortion(40, 20),
                }),
            new(
                "crostata-crema-cacao",
                CarbGuideCategoryId.Desserts,
                140,
                new(
                    "Crostata con crema al cacao",
                    "Cocoa cream tart"),
                new[]
                {
                    new CarbGuidePortion(50, 33),
                }),
            new(
                "cornetto",
                CarbGuideCategoryId.Desserts,
                150,
                new(
                    "Cornetto",
                    "Croissant"),
                new[]
                {
                    new CarbGuidePortion(50, 29),
                }),
            new(
                "cannolo-crema",
                CarbGuideCategoryId.Desserts,
                160,
                new(
                    "Cannolo con crema",
                    "Cream-filled cannolo"),
                new[]
                {
                    new CarbGuidePortion(50, 21),
                }),
            new(
                "krapfen-crema",
                CarbGuideCategoryId.Desserts,
                170,
                new(
                    "Krapfen con crema",
                    "Cream-filled doughnut"),
                new[]
                {
                    new CarbGuidePortion(100, 56),
                }),
            new(
                "marmellata-senza-zucchero",
                CarbGuideCategoryId.Desserts,
                180,
                new(
                    "Marmellata senza zucchero",
                    "Sugar-free jam"),
                new[]
                {
                    new CarbGuidePortion(10, 4),
                    new CarbGuidePortion(20, 8),
                    new CarbGuidePortion(30, 12),
                }),
            new(
                "marmellata",
                CarbGuideCategoryId.Desserts,
                190,
                new(
                    "Marmellata",
                    "Jam"),
                new[]
                {
                    new CarbGuidePortion(10, 6),
                    new CarbGuidePortion(20, 12),
                    new CarbGuidePortion(30, 18),
                }),
        ];
    }
}
