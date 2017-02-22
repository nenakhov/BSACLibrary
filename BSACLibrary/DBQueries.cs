﻿using BSACLibrary.Properties;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace BSACLibrary
{
    public class DBQueries
    {

        MainWindow mWin = MainWindow.AppWindow;

        public bool CheckIfConnectionExists()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Globals.connectionString))
                {
                    //Открываем соединение
                    conn.Open();

                    //Отмечаем в глобальной переменной что есть связь с БД
                    Globals.isConnected = true;

                    //Закроем соединение с БД и выгрузим лишнее из памяти
                    conn.Dispose();
                    return true;
                }
            }
            //В этом блоке перехватываем возможные ошибки в процессе соединения
            catch (MySqlException ex)
            {
                //Unable to connect to any of the specified MySQL hosts.
                if (ex.Message == "Unable to connect to any of the specified MySQL hosts.")
                {
                    MessageBox.Show("Проверьте настройки подключения к MySQL", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                //Отметим что нет соединения с БД
                Globals.isConnected = false;
                return false;
            }
        }

        //Метод выполняющий SQL запрос, не возвращает результата
        public void Execute(string query)
        {
            try
            {
                if (query != null)
                {
                    using (MySqlConnection conn = new MySqlConnection(Globals.connectionString))
                    {
                        //Открываем соединение
                        conn.Open();

                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.ExecuteNonQuery();
                        //Закроем соединение с БД и выгрузим лишнее из памяти
                        conn.Dispose();
                    }
                }
            }
            //В этом блоке перехватываем возможные ошибки
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Отдельный метод для обновления DataGrid таблицы в редакторе
        public void UpdateDataGrid()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Globals.connectionString))
                {
                    //Открываем соединение
                    conn.Open();

                    //Отправка запроса на обновление списка изданий из БД
                    MySqlDataAdapter newDataAdapter = new MySqlDataAdapter("SELECT id,publication,is_magazine,date,issue_number,file_path FROM " + Settings.Default.dbTableName, conn);

                    DataSet newDataSet = new DataSet();
                    newDataAdapter.Fill(newDataSet, "dbBinding");
                    //Заполняем таблицу в редакторе
                    mWin.dbDataGrid.DataContext = newDataSet;
                    //Обновим информацию о файлах для поиска
                    Initialize.UpdateFilesDescription();
                    //Закроем соединение с БД и выгрузим лишнее из памяти
                    conn.Dispose();
                }
            }
            //В этом блоке перехватываем возможные ошибки
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Метод выполняющий команду и возвращающий результат в виде массива
        public List<pdfDescription> ExecuteAndReadFilesDescription(string query)
        {
            try
            {
                if (query != null)
                {
                    using (MySqlConnection conn = new MySqlConnection(Globals.connectionString))
                    {
                        //Открываем соединение
                        conn.Open();

                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.ExecuteNonQuery();
                        MySqlDataReader Reader;
                        Reader = cmd.ExecuteReader();
                        //Cоздаем необходимые переменные
                        List<pdfDescription> newList = new List<pdfDescription>();

                        while (Reader.Read())
                        {
                            //Присвоими полученные от MySQL значения соответсвующему классу
                            pdfDescription curFile = new pdfDescription();

                            curFile.id = Convert.ToInt32(Reader.GetString(0));
                            curFile.publication = Reader.GetString(1);
                            curFile.is_magazine = Convert.ToBoolean(Reader.GetString(2));
                            curFile.date = Convert.ToDateTime(Reader.GetString(3));
                            curFile.issue_number = Convert.ToInt32(Reader.GetString(4));
                            curFile.file_path = Reader.GetString(5);

                            //И запишем данные в массив
                            newList.Add(curFile);
                        }
                        //Закроем соединение с БД и выгрузим лишнее из памяти
                        conn.Dispose();
                        return newList;
                    }
                }
                else return null;
            }
            //В этом блоке перехватываем возможные ошибки
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        //Метод для создания БД и таблицы, если они отсутсвуют
        public void DataBaseCreate()
        {
            try
            {
                //Откроем новое соединение
                using (MySqlConnection conn = new MySqlConnection(Globals.connectionString))
                {
                    conn.Open();

                    //Запрос для создания базы данных
                    //Если такая БД уже есть ошибки не будет (IF NOT EXISTS)
                    string query = "CREATE DATABASE IF NOT EXISTS " +
                                                "`" + Settings.Default.dbName +
                                                "` CHARACTER SET cp1251 COLLATE cp1251_general_ci;";
                    //Отправляем запрос
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();

                    //Выбираем БД с которой будем работать
                    query = "USE " + Settings.Default.dbName + ";";
                    //Отправляем запрос
                    cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();

                    //Запрос для создания таблицы
                    //Если такая таблица уже есть в БД ошибки не будет (IF NOT EXISTS)
                    query = "CREATE TABLE IF NOT EXISTS `" +
                                        Settings.Default.dbTableName + //Имя таблицы 
                                        "` (" +
                                        "`id` INT(6) NOT NULL AUTO_INCREMENT, " + //Автоинкрементирующееся поле id 
                                        "`publication` VARCHAR(255) NOT NULL, " + //Название издания
                                        "`is_magazine` TINYINT(1) NOT NULL, " + //Является ли журналом или газетой
                                        "`date` DATETIME NOT NULL, " + //Дата выхода издания
                                        "`issue_number` INT(6) NOT NULL, " + //Порядковый номер издания
                                        "`file_path` VARCHAR(255) NOT NULL, " + //Ссылка на файл
                                        "PRIMARY KEY(`id`) " +
                                        ") ";
                    //Отправляем запрос
                    cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();

                    Globals.connectionString = ("server=" + Settings.Default.dbServerIP +
                        ";port=" + Settings.Default.dbServerPort +
                        ";uid=" + Settings.Default.dbUsername +
                        ";pwd=" + Settings.Default.dbPassword +
                        ";database=" + Settings.Default.dbName +
                        ";charset=cp1251;convert zero datetime=true;");

                    //Закроем соединение с БД и выгрузим лишнее из памяти
                    conn.Dispose();
                }
            }
            //В этом блоке перехватываем возможные ошибки
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}