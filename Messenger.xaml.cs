﻿using System;
using System.Net;
using System.Numerics;
using System.Windows;

namespace csGameIorga
{
    public class Command
    {
        public string Nickname;
        public string Padding;
        public Vector2 Coordinates;
        public string Type;
        public Command(string rawData)
        {
            var splitData = rawData.Split(";");
            Nickname = splitData[0];
            Padding = splitData[1];
            var x  = int.Parse(splitData[2]);
            var y = int.Parse(splitData[3]);
            Type = splitData.Length >= 5 ? splitData[4] : "Move";
            Coordinates = new Vector2(x, y);
        }
    }
    /// <summary>
    /// Interaction logic for Messenger.xaml
    /// </summary>
    public partial class Messenger
    {
        private readonly Comunicator _comunicator;
        private readonly Board _board;
        public Messenger()
        {
            InitializeComponent();
            _comunicator = new Comunicator(this, Properties.Settings.Default.nickname, Properties.Settings.Default.localPort);
            _board = new Board(this);
            _board.Show();
        }

        private static IPEndPoint GenerateValidEndPoint(string rawIp, string rawPort, string defaultIp="127.0.0.1", int defaultPort=5000)
        {
            IPEndPoint newEp;
            var newIp = IPAddress.Parse(defaultIp);
            var newPort = defaultPort;
            
            try
            {
                newIp = IPAddress.Parse(rawIp);
                newPort = int.Parse(rawPort);
            }
            catch (FormatException) {}
            finally
            {
                newEp = new IPEndPoint(newIp, newPort);
            }
            
            return newEp;
        }
        
        private void SendData(object sender, RoutedEventArgs e)
        {
            _comunicator.Send(m_userInput.Text);
        }

        private void UpdateLocalEndPoint(object sender, RoutedEventArgs e)
        {
            var rawNewIp = _localIp.Text;
            var rawNewPort = _localPort.Text;
            var newEndPoint = GenerateValidEndPoint(rawNewIp, rawNewPort, IPAddress.Loopback.ToString());
            this._comunicator.UpdateLocalEndPoint(newEndPoint);
            // updating the fields with the correct values
            UpdateLocalEndPoint(newEndPoint);
        }        
        
        private void UpdateRemoteEndPoint(object sender, RoutedEventArgs e)
        {
            var rawNewIp = _remoteIp.Text;
            var rawNewPort = _remotePort.Text;
            var newEndPoint = GenerateValidEndPoint(rawNewIp, rawNewPort, IPAddress.Any.ToString());
            this._comunicator.UpdateRemoteEndPoint(newEndPoint);
            
            // updating the fields with the correct values
            UpdateRemoteEndPoint(newEndPoint);
        }

        public void UpdateNickname(string nickname)
        {
            _nickname.Text = nickname;
        }
        private void Nickname_leave(object sender, RoutedEventArgs e)
        {
            _comunicator.Nickname = _nickname.Text;
        }

        public void UpdateLocalEndPoint(IPEndPoint newEndPoint)
        {
            _localIp.Text = newEndPoint.Address.ToString();
            _localPort.Text = newEndPoint.Port.ToString();
        }
        public void UpdateRemoteEndPoint(IPEndPoint newEndPoint)
        {
            _remoteIp.Text = newEndPoint.Address.ToString();
            _remotePort.Text = newEndPoint.Port.ToString();
        }

        private void _saveState()
        {
            Properties.Settings.Default["localPort"] = _comunicator.remoteEndPoint.Port;
            Properties.Settings.Default["nickName"] = _comunicator.Nickname;
            Properties.Settings.Default.Save();
        }

        public void Execute(Command command, IPEndPoint sender)
        {
            var status = command.Nickname != _comunicator.Nickname ? "mismatched nickname" : _board.Move(command.Coordinates);
            Logger($"{(status.Length < 1 ? "Succeeded" : "Failed")} to run <{command.Type}> command with parameters " +
                   $"<x:{command.Coordinates.X},y:{command.Coordinates.Y}> sent by {command.Nickname}({sender}){(status.Length < 1 ? "" : " because of <" + status + ">")}\n");
        }
        
        public void Logger(string message)
        {
            m_logger.Text += message;
        }

        public void Shutdown()
        {
            _saveState();
            _board.Close();
            _comunicator.Shutdown();
            this.Close();
        }
        
        private void Window_OnClosed(object? sender, EventArgs e)
        {
            this.Shutdown();
        }
    }
}
