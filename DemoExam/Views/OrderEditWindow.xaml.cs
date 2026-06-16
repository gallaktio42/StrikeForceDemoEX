using DemoExam.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DemoExam.Models;
using System.Linq;

namespace DemoExam.Views
{
    /// <summary>
    /// Логика взаимодействия для OrderEditWindow.xaml
    /// </summary>
    public partial class OrderEditWindow : Window
    {
        private static OrderEditWindow? _instance;
        private Order _editingOrder;
        private readonly AppDbContext _context;

        public OrderEditWindow(Order order, AppDbContext context)
        {
            if (_instance != null)
            {
                _instance.Activate();
                Close();
                return;
            }

            InitializeComponent();
            _context = context;
            _editingOrder = order;
            _instance = this;
            Closed += (s, e) => _instance = null;

            LoadComboBoxes();

            if (_editingOrder != null)
                LoadOrderData();
            else
                dpOrderDate.SelectedDate = DateTime.Today;
        }

        private void LoadComboBoxes()
        {
            // Загрузка статусов из БД
            cmbStatus.ItemsSource = _context.OrderStatuses.OrderBy(s => s.StatusID).ToList();
            cmbStatus.DisplayMemberPath = "StatusName";
            cmbStatus.SelectedValuePath = "StatusID";

            // Загрузка пунктов выдачи из БД
            cmbPickupAddress.ItemsSource = _context.PickupPoints.OrderBy(p => p.PointID).ToList();
            cmbPickupAddress.DisplayMemberPath = "Address";
            cmbPickupAddress.SelectedValuePath = "PointID";
        }

        private void LoadOrderData()
        {
            txtOrderNumber.Text = _editingOrder!.OrderNumber;
            cmbStatus.SelectedValue = _editingOrder.StatusID;
            cmbPickupAddress.SelectedValue = _editingOrder.PointID;
            dpOrderDate.SelectedDate = _editingOrder.OrderDate;
            dpIssueDate.SelectedDate = _editingOrder.IssueDate;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOrderNumber.Text))
            {
                MessageBox.Show("Артикул заказа не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус заказа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbPickupAddress.SelectedValue == null)
            {
                MessageBox.Show("Выберите пункт выдачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingOrder == null)
                {
                    _editingOrder = new Order();
                    _context.Orders.Add(_editingOrder);
                }

                _editingOrder.OrderNumber = txtOrderNumber.Text.Trim();
                _editingOrder.StatusID = (int)cmbStatus.SelectedValue;
                _editingOrder.PointID = (int)cmbPickupAddress.SelectedValue;
                _editingOrder.OrderDate = dpOrderDate.SelectedDate ?? DateTime.Today;
                _editingOrder.IssueDate = dpIssueDate.SelectedDate;

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
