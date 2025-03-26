using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Практика_по_архиву.DB_;

namespace Практика_по_архиву
{
    /// <summary>
    /// Логика взаимодействия для ProductEditWindow.xaml
    /// </summary>
    public class ProductMaterialViewModel
    {
        public int MaterialID { get; set; }
        public string MaterialTitle { get; set; }
        public int Count { get; set; }
    }

    public partial class ProductEditWindow : Window
    {
        private readonly ПрактикаEntities _dbContext;
        private Product _product;
        private readonly bool _isEditing;
        private readonly List<ProductMaterialViewModel> _materials;
        private string _imagePath;

        public ProductEditWindow(ПрактикаEntities dbContext, Product product = null)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _product = product;
            _isEditing = product != null;
            _materials = new List<ProductMaterialViewModel>();

            ProductTypeComboBox.ItemsSource = _dbContext.ProductType.ToList();
            ProductTypeComboBox.DisplayMemberPath = "Title";

            MaterialComboBox.ItemsSource = _dbContext.Material.ToList();

            if (_isEditing)
            {
                Title = "Редактирование продукции";
                DeleteButton.Visibility = Visibility.Visible;
                LoadProductData();
            }
            else
            {
                Title = "Добавление продукции";
            }
        }

        private void LoadProductData()
        {
            ArticleNumberTextBox.Text = _product.ArticleNumber;
            TitleTextBox.Text = _product.Title;
            ProductTypeComboBox.SelectedItem = _dbContext.ProductType.Find(_product.ProductTypeID);
            ProductionPersonCountTextBox.Text = _product.ProductionPersonCount.HasValue ? _product.ProductionPersonCount.Value.ToString() : "";
            ProductionWorkshopNumberTextBox.Text = _product.ProductionWorkshopNumber.HasValue ? _product.ProductionWorkshopNumber.Value.ToString() : "";
            MinCostForAgentTextBox.Text = _product.MinCostForAgent.HasValue ? _product.MinCostForAgent.Value.ToString() : "";
            DescriptionTextBox.Text = _product.Description;
            _imagePath = _product.Image;

            if (!string.IsNullOrEmpty(_imagePath))
            {
                ProductImage.Source = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri($"{_imagePath}", UriKind.Relative));
            }

            var productMaterials = _dbContext.ProductMaterial
                .Where(pm => pm.ProductID == _product.ID)
                .ToList();
            foreach (var pm in productMaterials)
            {
                var material = _dbContext.Material.Find(pm.MaterialID);
                if (material != null)
                {
                    _materials.Add(new ProductMaterialViewModel
                    {
                        MaterialID = material.ID,
                        MaterialTitle = material.Title,
                        Count = (int)(pm.Count ?? 0)
                    });
                }
            }
            MaterialsListView.ItemsSource = _materials;
        }

        private void ChangeImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    string newPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", System.IO.Path.GetFileName(filename));
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(newPath));
                    File.Copy(filename, newPath, true);
                    _imagePath = newPath;
                    ProductImage.Source = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri(newPath, UriKind.Absolute));
                }
                
            }
        }


        private void RemoveMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductMaterialViewModel material)
            {
                _materials.Remove(material);
                MaterialsListView.ItemsSource = null;
                MaterialsListView.ItemsSource = _materials;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var validator = new ProductValidator(_dbContext);

            // Валидация данных продукта
            var validationResult = validator.ValidateProductData(
                ArticleNumberTextBox.Text,
                TitleTextBox.Text,
                ProductTypeComboBox.SelectedItem as ProductType,
                ProductionPersonCountTextBox.Text,
                ProductionWorkshopNumberTextBox.Text,
                MinCostForAgentTextBox.Text);

            if (!validationResult.IsValid)
            {
                MessageBox.Show(validationResult.ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка уникальности артикула
            string articleNumber = ArticleNumberTextBox.Text;
            var uniquenessResult = validator.CheckArticleNumberUniqueness(articleNumber, _isEditing, _product);
            if (!uniquenessResult.IsUnique)
            {
                MessageBox.Show(uniquenessResult.ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сохранение продукта
            int personCount = int.Parse(ProductionPersonCountTextBox.Text);
            int workshopNumber = int.Parse(ProductionWorkshopNumberTextBox.Text);
            decimal minCost = decimal.Parse(MinCostForAgentTextBox.Text);

            if (_isEditing)
            {
                _product.ArticleNumber = articleNumber;
                _product.Title = TitleTextBox.Text;
                _product.ProductTypeID = (ProductTypeComboBox.SelectedItem as ProductType).ID;
                _product.ProductionPersonCount = personCount;
                _product.ProductionWorkshopNumber = workshopNumber;
                _product.MinCostForAgent = minCost;
                _product.Description = DescriptionTextBox.Text;
                _product.Image = _imagePath;

                var oldMaterials = _dbContext.ProductMaterial.Where(pm => pm.ProductID == _product.ID).ToList();
                _dbContext.ProductMaterial.RemoveRange(oldMaterials);
            }
            else
            {
                _product = new Product
                {
                    ArticleNumber = articleNumber,
                    Title = TitleTextBox.Text,
                    ProductTypeID = (ProductTypeComboBox.SelectedItem as ProductType).ID,
                    ProductionPersonCount = personCount,
                    ProductionWorkshopNumber = workshopNumber,
                    MinCostForAgent = minCost,
                    Description = DescriptionTextBox.Text,
                    Image = _imagePath
                };
                _dbContext.Product.Add(_product);
            }

            _dbContext.SaveChanges();

            foreach (var material in _materials)
            {
                _dbContext.ProductMaterial.Add(new ProductMaterial
                {
                    ProductID = _product.ID,
                    MaterialID = material.MaterialID,
                    Count = material.Count
                });
            }

            _dbContext.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            var validator = new ProductValidator(_dbContext);
            var validationResult = validator.ValidateMaterialAddition(
                MaterialComboBox.SelectedItem as Material,
                MaterialCountTextBox.Text);

            if (!validationResult.IsValid)
            {
                MessageBox.Show(validationResult.ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedMaterial = MaterialComboBox.SelectedItem as Material;
            int count = int.Parse(MaterialCountTextBox.Text);

            _materials.Add(new ProductMaterialViewModel
            {
                MaterialID = selectedMaterial.ID,
                MaterialTitle = selectedMaterial.Title,
                Count = count
            });
            MaterialsListView.ItemsSource = null;
            MaterialsListView.ItemsSource = _materials;
            MaterialComboBox.SelectedIndex = -1;
            MaterialCountTextBox.Text = "";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dbContext.ProductSale.Any(ps => ps.ProductID == _product.ID))
            {
                MessageBox.Show("Нельзя удалить продукт, так как у него есть история продаж.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этот продукт?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var materials = _dbContext.ProductMaterial.Where(pm => pm.ProductID == _product.ID).ToList();
                _dbContext.ProductMaterial.RemoveRange(materials);

                // Удаляем продукт
                _dbContext.Product.Remove(_product);
                _dbContext.SaveChanges();

                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
