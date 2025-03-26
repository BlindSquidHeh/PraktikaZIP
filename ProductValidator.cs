using System;
using System.Collections.Generic;
using System.Linq;
using Практика_по_архиву.DB_;

namespace Практика_по_архиву
{
    public class ProductValidator
    {
        private readonly ПрактикаEntities _dbContext;

        public ProductValidator(ПрактикаEntities dbContext)
        {
            _dbContext = dbContext;
        }

        public (bool IsValid, string ErrorMessage) ValidateProductData(
            string articleNumber,
            string title,
            ProductType productType,
            string productionPersonCountText,
            string productionWorkshopNumberText,
            string minCostForAgentText)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(articleNumber))
                return (false, "Артикул не может быть пустым.");

            if (string.IsNullOrWhiteSpace(title))
                return (false, "Наименование не может быть пустым.");

            if (productType == null)
                return (false, "Тип продукта должен быть выбран.");

            // Проверка числовых полей
            if (!int.TryParse(productionPersonCountText, out int personCount) || personCount < 0)
                return (false, "Количество человек для производства должно быть неотрицательным числом.");

            if (!int.TryParse(productionWorkshopNumberText, out int workshopNumber) || workshopNumber < 0)
                return (false, "Номер цеха должен быть неотрицательным числом.");

            if (!decimal.TryParse(minCostForAgentText, out decimal minCost) || minCost < 0)
                return (false, "Минимальная стоимость для агента должна быть неотрицательной.");

            return (true, string.Empty);
        }

        public (bool IsUnique, string ErrorMessage) CheckArticleNumberUniqueness(
            string articleNumber,
            bool isEditing,
            Product product)
        {
            Product existingProduct;
            if (isEditing)
            {
                existingProduct = _dbContext.Product
                    .FirstOrDefault(p => p.ArticleNumber == articleNumber && p.ID != product.ID);
            }
            else
            {
                existingProduct = _dbContext.Product
                    .FirstOrDefault(p => p.ArticleNumber == articleNumber);
            }

            if (existingProduct != null)
                return (false, "Продукт с таким артикулом уже существует.");

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidateMaterialAddition(
            Material selectedMaterial,
            string materialCountText)
        {
            if (selectedMaterial == null)
                return (false, "Материал должен быть выбран.");

            if (!int.TryParse(materialCountText, out int count) || count <= 0)
                return (false, "Количество материала должно быть положительным числом.");

            return (true, string.Empty);
        }
    }
}