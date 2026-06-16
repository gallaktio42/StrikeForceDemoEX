using DemoExam.Data;
using DemoExam.Helpers;
using DemoExam.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DemoExam.Views
{
    public partial class ProductEditWindow : Window
    {
        private Product _editingProduct;
        private readonly AppDbContext _context;
        private string? _newImagePath;

        public ProductEditWindow(Product product, AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            _editingProduct = product;
            LoadComboBoxes();
            if (product != null) LoadProductData();
        }

        private void LoadComboBoxes()
        {
            cmbCategory.ItemsSource = _context.Categories.ToList();
            cmbManufacturer.ItemsSource = _context.Manufacturers.ToList();
            cmbSupplier.ItemsSource = _context.Suppliers.ToList();
        }

        private void LoadProductData()
        {
            txtName.Text = _editingProduct!.Name;
            cmbCategory.SelectedValue = _editingProduct.CategoryID;
            txtDescription.Text = _editingProduct.Description;
            cmbManufacturer.SelectedValue = _editingProduct.ManufacturerID;
            cmbSupplier.SelectedValue = _editingProduct.SupplierID;
            txtPrice.Text = _editingProduct.Price.ToString(CultureInfo.InvariantCulture);
            txtUnit.Text = _editingProduct.Unit;
            txtStock.Text = _editingProduct.StockQuantity.ToString();
            txtDiscount.Text = _editingProduct.Discount.ToString(CultureInfo.InvariantCulture);
            txtArticleNumber.Text = _editingProduct.Article;
            LoadImageWithFallback(_editingProduct.ImagePath);
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.png" };
            if (dialog.ShowDialog() == true)
            {
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dialog.FileName);
                string destPath = Path.Combine(imagesFolder, fileName);
                File.Copy(dialog.FileName, destPath, overwrite: true);
                _newImagePath = fileName;
                imgPreview.Source = new BitmapImage(new Uri(destPath, UriKind.Absolute));
            }
        }

        private void LoadImageWithFallback(string? imageFileName)
        {
            string? fullPath = ImageHelpers.GetFullPath(imageFileName);
            if (fullPath != null)
            {
                imgPreview.Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
            }
            else
            {
                string? fallback = ImageHelpers.GetFallbackPath();
                imgPreview.Source = fallback != null ? new BitmapImage(new Uri(fallback, UriKind.Absolute)) : null;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Цена
                string priceText = txtPrice.Text.Trim().Replace(',', '.');
                if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price < 0)
                {
                    MessageBox.Show($"Некорректная цена: '{txtPrice.Text}'. Используйте цифры и разделитель (точку или запятую).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Количество
                string stockText = txtStock.Text.Trim();
                if (!int.TryParse(stockText, out int stock) || stock < 0)
                {
                    MessageBox.Show($"Некорректное количество: '{txtStock.Text}'. Введите целое число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Скидка
                string discountText = txtDiscount.Text.Trim().Replace(',', '.');
                if (!decimal.TryParse(discountText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal disc) || disc < 0 || disc > 100)
                {
                    MessageBox.Show($"Некорректная скидка: '{txtDiscount.Text}'. От 0 до 100.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Наименование товара обязательно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbCategory.SelectedValue == null || cmbManufacturer.SelectedValue == null || cmbSupplier.SelectedValue == null)
                {
                    MessageBox.Show("Заполните категорию, производителя и поставщика.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_editingProduct == null)
                {
                    _editingProduct = new Product();
                    _context.Products.Add(_editingProduct);
                }

                _editingProduct.Name = txtName.Text;
                _editingProduct.Article = txtArticleNumber.Text.Trim();
                _editingProduct.CategoryID = (int)cmbCategory.SelectedValue;
                _editingProduct.Description = txtDescription.Text;
                _editingProduct.ManufacturerID = (int)cmbManufacturer.SelectedValue;
                _editingProduct.SupplierID = (int)cmbSupplier.SelectedValue;
                _editingProduct.Price = price;
                _editingProduct.Unit = txtUnit.Text;
                _editingProduct.StockQuantity = stock;
                _editingProduct.Discount = disc;

                if (string.IsNullOrEmpty(_editingProduct.Article))
                {
                    MessageBox.Show("Артикул товара обязателен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Обработка изображения
                if (!string.IsNullOrEmpty(_newImagePath))
                {
                    if (!string.IsNullOrEmpty(_editingProduct.ImagePath))
                    {
                        string oldFullPath = Path.Combine(ImageHelpers.GetImagesFolder(), _editingProduct.ImagePath);
                        if (File.Exists(oldFullPath)) File.Delete(oldFullPath);
                    }
                    _editingProduct.ImagePath = _newImagePath;
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}