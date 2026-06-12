using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using perfumery.Context;
using System;
using Tmds.DBus.Protocol;
using static System.Collections.Specialized.BitVector32;

namespace perfumery;

public partial class LoginWindow : Window
{
    private bool _isPasswordVisible = false;
    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        var login = (LoginBox.Text ?? "").Trim();
        var password = PasswordBox.Text ?? "";
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            await MessageBoxManager
                 .GetMessageBoxStandard("Ошибка", "Введите логин и пароль", ButtonEnum.Ok)
                 .ShowAsync();
            return;
        }
        try
        {
            await using var db = new PerfumeryContext();
            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == login && u.Password == password);
            if (user == null)
            {
                await MessageBoxManager
                .GetMessageBoxStandard("Ошибка", "Неверный логин или пароль", ButtonEnum.Ok)
                .ShowAsync();
                return;
            }
            var session = new UserSession(user);
            var main = new MainWindow(session);
            main.Show();
            Close();
        }catch(Exception ex)
        {
            await MessageBoxManager
               .GetMessageBoxStandard("Ошибка", "Ошибка входа" + ex, ButtonEnum.Ok)
               .ShowAsync();
            return;
        }
    }

    private void Guest_Click(object? sender, RoutedEventArgs e)
    {
        var main = new MainWindow();
        main.Show();
        Close();
    }

    private void Toggle_Password(object? sender, RoutedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;

        if (_isPasswordVisible)
        {
            PasswordVisibleBox.Text = PasswordBox.Text;
            PasswordVisibleBox.IsVisible = true;
            PasswordBox.IsVisible = false;
        }
        else
        {
            PasswordBox.Text = PasswordVisibleBox.Text;
            PasswordBox.IsVisible = true;
            PasswordVisibleBox.IsVisible = false;
        }
    }
}