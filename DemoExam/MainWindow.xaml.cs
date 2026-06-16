using DemoExam.Data;
using DemoExam.Helpers;
using DemoExam.Models;
using DemoExam.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DemoExam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private ObservableCollection<Product> _products;
        private ICollectionView _productsView;
        private readonly AppDbContext _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _currentUser = LoginWindow.CurrentUser;
            lblUser.Text = _currentUser?.FullName ?? "Гость";

            if (_currentUser?.Role?.RoleName != null)
            {
                string role = _currentUser.Role.RoleName;
                if (role.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
                {
                    searchPanel.Visibility = Visibility.Visible;
                    btnAddProduct.Visibility = Visibility.Visible;
                    btnEditProduct.Visibility = Visibility.Visible;
                    btnDeleteProduct.Visibility = Visibility.Visible;
                    btnOrders.Visibility = Visibility.Visible;
                }
                else if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                {
                    searchPanel.Visibility = Visibility.Visible;
                    btnOrders.Visibility = Visibility.Visible;
                }
                else if (role.Equals("Client", StringComparison.OrdinalIgnoreCase) || role.Equals("Guest", StringComparison.OrdinalIgnoreCase))
                {
                    searchPanel.Visibility = Visibility.Collapsed;
                }
            }

            LoadProducts();
            LoadSupplierFilter();
        }

        private void LoadProducts()
        {
            var list = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .ToList();
            _products = new ObservableCollection<Product>(list);
            _productsView = CollectionViewSource.GetDefaultView(_products);
            dgProducts.ItemsSource = _productsView;
        }

        private void LoadSupplierFilter()
        {
            var suppliers = _context.Suppliers.ToList();
            suppliers.Insert(0, new Supplier { SupplierID = 0, SupplierName = "Все поставщики" });
            cmbSupplierFilter.ItemsSource = suppliers;
            cmbSupplierFilter.DisplayMemberPath = "SupplierName";
            cmbSupplierFilter.SelectedValuePath = "SupplierID";
            cmbSupplierFilter.SelectedIndex = 0;
        }

        private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selected)
            {
                lblSelectedProductName.Text = selected.Name;
                // Загрузка изображения (с fallback)
                LoadProductImage(selected.ImagePath);
            }
            else
            {
                lblSelectedProductName.Text = "(не выбран)";
                imgSelectedProduct.Source = null;
            }
        }

        private void LoadProductImage(string? imagePath)
        {
            string? fullPath = ImageHelpers.GetFullPath(imagePath);
            if (fullPath != null)
            {
                imgSelectedProduct.Source = new BitmapImage(new Uri(fullPath));
            }
            else
            {
                string? fallback = ImageHelpers.GetFallbackPath();
                imgSelectedProduct.Source = fallback != null ? new BitmapImage(new Uri(fallback)) : null;
            }
        }

        private void Img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {e.ErrorException.Message}");
            string? fallback = ImageHelpers.GetFallbackPath();
            if (fallback != null)
                ((Image)sender).Source = new BitmapImage(new Uri(fallback, UriKind.Absolute));
        }

        private void ProductImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var img = sender as Image;
            if (img != null)
            {
                string? fallback = ImageHelpers.GetFallbackPath();
                if (fallback != null)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fallback, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    img.Source = bitmap;
                }
                else
                {
                    img.Source = null;
                }
            }
        }

        // Фильтрация и поиск
        private void ApplyFilterAndSort()
        {
            if (_productsView == null) return;

            _productsView.Filter = item =>
            {
                var p = item as Product;
                if (p == null) return false;

                // Поиск по тексту
                bool matchSearch = string.IsNullOrEmpty(txtSearch.Text) ||
                    p.Name.Contains(txtSearch.Text) ||
                    (p.Description != null && p.Description.Contains(txtSearch.Text)) ||
                    (p.Category != null && p.Category.CategoryName.Contains(txtSearch.Text)) ||
                    (p.Manufacturer != null && p.Manufacturer.ManufacturerName.Contains(txtSearch.Text)) ||
                    (p.Supplier != null && p.Supplier.SupplierName.Contains(txtSearch.Text));

                // Фильтр по поставщику
                bool matchSupplier = true;
                if (cmbSupplierFilter.SelectedItem is Supplier supplier && supplier.SupplierID != 0)
                {
                    matchSupplier = p.SupplierID == supplier.SupplierID;
                }
                return matchSearch && matchSupplier;
            };

            // Сортировка
            _productsView.SortDescriptions.Clear();
            if (cmbSortStock.SelectedIndex == 1)
                _productsView.SortDescriptions.Add(new SortDescription("StockQuantity", ListSortDirection.Ascending));
            else if (cmbSortStock.SelectedIndex == 2)
                _productsView.SortDescriptions.Add(new SortDescription("StockQuantity", ListSortDirection.Descending));

            _productsView.Refresh();
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ApplyFilterAndSort();
        private void CmbSupplierFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => ApplyFilterAndSort();
        private void CmbSortStock_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => ApplyFilterAndSort();

        private void DgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        { // Двойной клик для редактирования
            if (_currentUser?.Role?.RoleName != null &&
                _currentUser.Role.RoleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase) &&
                dgProducts.SelectedItem is Product p)
                EditProduct(p);
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var editWin = new ProductEditWindow(null, _context);
            if (editWin.ShowDialog() == true)
                RefreshProducts();
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product p)
                EditProduct(p);
        }

        private void EditProduct(Product product)
        {
            var editWin = new ProductEditWindow(product, _context);
            if (editWin.ShowDialog() == true)
                RefreshProducts();
        }

        private async void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is not Product p) return;
            // Проверка на наличие в заказах
            bool inOrder = await _context.OrderItems.AnyAsync(oi => oi.ProductID == p.ProductID);
            if (inOrder)
            {
                MessageBox.Show("Невозможно удалить товар, который присутствует в заказах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Удалить товар \"{p.Name}\"?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Удаление изображения с диска
                if (!string.IsNullOrEmpty(p.ImagePath) && System.IO.File.Exists(p.ImagePath))
                    System.IO.File.Delete(p.ImagePath);
                _context.Products.Remove(p);
                await _context.SaveChangesAsync();
                RefreshProducts();
            }
        }

        private void RefreshProducts()
        {
            var updated = _context.Products.Include(p => p.Category).Include(p => p.Manufacturer).Include(p => p.Supplier).ToList();
            _products.Clear();
            foreach (var p in updated) _products.Add(p);
            ApplyFilterAndSort();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            var ordersWin = new OrderWindow(_currentUser, _context);
            ordersWin.ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}