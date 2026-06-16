using CloudinaryDotNet.Actions;
using DemoExam.Data;
using DemoExam.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoExam.Views
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private readonly User _currentUser;
        private readonly AppDbContext _context;
        private ObservableCollection<Order> _orders;

        public OrderWindow(User currentUser, AppDbContext context)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _context = context;

            // Принудительная загрузка роли, если она ещё не загружена
            if (_currentUser != null && _currentUser.Role == null)
                _context.Entry(_currentUser).Reference(u => u.Role).Load();

            // Определение прав администратора
            bool isAdmin = _currentUser?.Role?.RoleName != null &&
                           _currentUser.Role.RoleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase);

            if (isAdmin)
            {
                btnAdd.Visibility = Visibility.Visible;
                btnEdit.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
            }
            else
            {
                btnAdd.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
            }

            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                _orders = new ObservableCollection<Order>(
                    _context.Orders
                        .Include(o => o.Status)
                        .Include(o => o.PickupPoint)
                        .ToList()
                );
                dgOrders.ItemsSource = _orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => this.Close();
        

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWin = new OrderEditWindow(null, _context);
                if (editWin.ShowDialog() == true) RefreshOrders();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна добавления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                try
                {
                    var editWin = new OrderEditWindow(selectedOrder, _context);
                    if (editWin.ShowDialog() == true) RefreshOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии окна редактирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ №{selectedOrder.OrderNumber}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Orders.Remove(selectedOrder);
                        _context.SaveChanges();
                        RefreshOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshOrders()
        {
            _orders.Clear();
            var updated = _context.Orders.Include(o => o.Status).Include(o => o.PickupPoint).ToList();
            foreach (var order in updated)
                _orders.Add(order);
        }

    }
}
