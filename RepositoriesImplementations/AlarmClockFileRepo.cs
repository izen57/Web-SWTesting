﻿using Exceptions.AlarmClockExceptions;

using Model;

using Repositories;

using Serilog;

using System.Drawing;

namespace RepositoriesImplementations
{
	public class AlarmClockFileRepo: IAlarmClockRepo
	{
		DirectoryInfo _isoStore;

		public AlarmClockFileRepo()
		{
			try
			{
				_isoStore = new DirectoryInfo("IsolatedStorage");
				string f = _isoStore.FullName;
				_isoStore.CreateSubdirectory("alarmclocks");
			}
			catch (Exception ex)
			{
				throw new AlarmClockCreateException(
					"Невозможно создать защищённое хранилище будильников.",
					ex
				);
			}
			Log.Logger.Information("AlarmClockFileRepo: Создано хранилище будильников.");
		}

		public AlarmClock Create(AlarmClock alarmClock)
		{
			string alarmClockFilePath = $"IsolatedStorage/alarmclocks/{alarmClock.Id}.txt";

			if (File.Exists(alarmClockFilePath))
			{
				Log.Logger.Error($"AlarmClockCreate: Файл alarmclocks/{alarmClock.Id}.txt уже существует.");
				throw new AlarmClockCreateException(
					$"Не удалось создать будильник {alarmClock.Id} для пользователя {alarmClock.OwnerId}.",
					new IOException("already exists")
				);
			}

			FileStream isoStream;
			try
			{
				isoStream = File.Create(alarmClockFilePath);
			}
			catch (Exception ex)
			{
				Log.Logger.Error($"AlarmClockCreate: Файл alarmclocks/{alarmClock.Id}.txt нельзя открыть.");
				throw new AlarmClockCreateException(
					$"Не удалось создать будильник {alarmClock.Id} для пользователя {alarmClock.OwnerId}.",
					ex
				);
			}

			using (StreamWriter writer = new(isoStream))
			{
				writer.WriteLine(alarmClock.OwnerId);
				writer.WriteLine(alarmClock.Name);
				writer.WriteLine($"{alarmClock.AlarmTime:yyyy-MM-dd HH:mm:ss}");
				writer.WriteLine(alarmClock.AlarmClockColor.Name);
				writer.WriteLine(alarmClock.IsWorking);
			}
			isoStream.Close();

			string userFilePath = $"IsolatedStorage/users/{alarmClock.OwnerId}.txt";
			try
			{
				List<string> previousUserLines = File.ReadAllLines(userFilePath).ToList();
				previousUserLines.Insert(previousUserLines.IndexOf("A") + 1, alarmClock.Id.ToString()!);
				File.WriteAllLines(userFilePath, previousUserLines);
			}
			catch (Exception ex)
			{
				File.Delete(alarmClockFilePath);
				Log.Logger.Error($"AlarmClockCreate: Файл users/{alarmClock.OwnerId}.txt нельзя открыть.");
				throw new AlarmClockCreateException(
					$"Не удалось создать будильник {alarmClock.Id} для пользователя {alarmClock.OwnerId}.",
					ex
				);
			}

			Log.Logger.Information("Создан файл будильника со следующей информацией:" +
				$"{alarmClock.OwnerId}," +
				$"{alarmClock.Name}," +
				$"{alarmClock.AlarmTime}," +
				$"{alarmClock.AlarmClockColor.Name}," +
				$"{alarmClock.IsWorking}."
			);
			return alarmClock;
		}

		public AlarmClock Edit(AlarmClock alarmClock)
		{
			FileStream isoStream;
			try
			{
				isoStream = new(
					$"IsolatedStorage/alarmclocks/{alarmClock.Id}.txt",
					FileMode.Create,
					FileAccess.Write
				);
			}
			catch (Exception ex)
			{
				Log.Logger.Error($"AlarmClockEdit: Файл alarmclocks/{alarmClock.Id}.txt не найден.");
				throw new AlarmClockEditException(
					$"Будильник {alarmClock.Id} пользователя {alarmClock.OwnerId} не найден.",
					ex
				);
			}

			using (StreamWriter writer = new(isoStream))
			{
				writer.WriteLine(alarmClock.OwnerId);
				writer.WriteLine(alarmClock.Name);
				writer.WriteLine($"{alarmClock.AlarmTime:yyyy-MM-dd HH:mm:ss}");
				writer.WriteLine(alarmClock.AlarmClockColor.Name);
				writer.WriteLine(alarmClock.IsWorking);
			}

			Log.Logger.Information("Файл будильника изменён. Новая информация:" +
				$"{alarmClock.OwnerId}," +
				$"{alarmClock.Name}," +
				$"{alarmClock.AlarmTime}," +
				$"{alarmClock.AlarmClockColor.Name}," +
				$"{alarmClock.IsWorking}."
			);

			isoStream.Close();
			return alarmClock;
		}

		public void Delete(Guid guid, Guid ownerId)
		{
			string alarmClockFilePath = $"IsolatedStorage/alarmclocks/{guid}.txt";
			if (File.Exists(alarmClockFilePath) == false)
			{
				Log.Logger.Error($"AlarmClockDelete: Файл alarmclocks/{guid}.txt не найден.");
				throw new AlarmClockDeleteException(
					$"Будильник {guid} не найден.",
					new FileNotFoundException(alarmClockFilePath)
				);
			}

			string userFilePath = $"IsolatedStorage/users/{ownerId}.txt";
			try
			{
				List<string> previousUserLines = File.ReadAllLines(userFilePath).ToList();
				previousUserLines.Remove(guid.ToString());
				File.WriteAllLines(userFilePath, previousUserLines);
			}
			catch (Exception ex)
			{
				Log.Logger.Error($"AlarmClockDelete: Файл users/{ownerId}.txt не найден.");
				throw new AlarmClockDeleteException(
					$"Пользователь {ownerId} не найден.",
					ex
				);
			}

			File.Delete(alarmClockFilePath);
			Log.Logger.Information($"AlarmClockDelete: Файл будильника {guid} удалён.");
		}

		public AlarmClock? GetAlarmClock(Guid guid)
		{
			FileStream isoStream;

			try
			{
				isoStream = new(
					$"IsolatedStorage/alarmclocks/{guid}.txt",
					FileMode.Open,
					FileAccess.Read
				);
			}
			catch (Exception ex)
			{
				Log.Logger.Error($"AlarmClockGet: Ошибка открытие файла alarmclocks/{guid}.txt.");
				return null;
				//throw new AlarmClockGetException(
				//	$"Будильник пользователя {_ownerId} на время {alarmTime:dd.MM.yyyy HH-mm-ss}. не найден",
				//	ex
				//);
			}

			using var readerStream = new StreamReader(isoStream);
			string? alarmClockOwnerId = readerStream.ReadLine();
			string? alarmClockName = readerStream.ReadLine();
			string? alarmClockTime = readerStream.ReadLine();
			string? alarmClockColor = readerStream.ReadLine();
			string? alarmClockWork = readerStream.ReadLine();
			if (alarmClockName == null || alarmClockTime == null || alarmClockColor == null || alarmClockWork == null || alarmClockOwnerId == null)
			{
				Log.Logger.Error($"AlarmClockGet: Ошибка разметки файла будильника {guid}.");
				throw new ArgumentNullException();
			}

			isoStream.Close();
			return new AlarmClock(
				guid,
				DateTime.Parse(alarmClockTime),
				alarmClockName,
				Guid.Parse(alarmClockOwnerId),
				Color.FromName(alarmClockColor),
				bool.Parse(alarmClockWork)
			);
		}

		public List<AlarmClock> GetAlarmClocks()
		{
			IEnumerable<string> filelist;
			List<AlarmClock> alarmClockList = new();
			try
			{
				filelist = Directory.EnumerateFiles("IsolatedStorage/alarmclocks/");
			}
			catch (Exception ex)
			{
				Log.Logger.Error("AlarmClockGet: Папка для будильников в защищённом хранилище не найдена.");
				return alarmClockList;
			}

			foreach (string fileName in filelist)
			{
				var alarmClock = GetAlarmClock(Guid.Parse(
					fileName
						.Replace("IsolatedStorage/alarmclocks/", "")
						.Replace(".txt", "")
				));
				alarmClockList.Add(alarmClock!);
			}

			return alarmClockList;
		}

		public List<AlarmClock> GetAlarmClocks(Guid ownerId)
		{
			List<AlarmClock> alarmClockList = new();
			FileStream fileStream = new(
				$"IsolatedStorage/users/{ownerId}.txt",
				FileMode.Open,
				FileAccess.Read
			);
			StreamReader streamReader = new(fileStream);

			while (streamReader.ReadLine() != "A")
				;
			string? alarmClockId;
			while ((alarmClockId = streamReader.ReadLine()) != "A")
				if (alarmClockId != null)
					alarmClockList.Add(GetAlarmClock(Guid.Parse(alarmClockId)!));

			fileStream.Close();
			return alarmClockList;
		}

		public List<AlarmClock> GetAlarmClocks(Guid ownerId, QueryStringParameters param)
		{
			return GetAlarmClocks(ownerId)
				.Skip((param.PageNumber - 1) * param.PageSize)
				.Take(param.PageSize)
				.ToList();
		}
	}
}