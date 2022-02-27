## Wallpapers

Консольное приложение, загружающее изображения из интернета 

## Конфигурация

Конфигурация находится в файле `appsettings.json` рядом с исполняемом файле:
```json
{
    "Serilog" : {
        "MinimalLevel": {
            "Default": "Debug",
            "Override": {
                "Microsoft": "Information",
                "System": "Warning"
            }
        }
    },
    "WallPaperConfig": {
      "ImageMax": 5,
      "YandexUrl": "https://yandex.ru/images/search?text=wallpapers&isize=large&p={0}",
      "WallpaperFolder": "C:\Users\Public\Pictures\Wallpapers",
      "WallpaperFileHistory": "C:\Users\Public\Pictures\image_list.json"
    }
}
```

- ImageMax  - Максимальное колво изображение, которое необходимо скачать и сохранить
- YandexUrl - Url yandex со страницей изображений по нему будет осуществляться поиск (значение по умолчанию: "https://yandex.ru/images/search?text=wallpapers&p={0}")
  - large - размер изображений
  - p - страница (будет инкрементироваться автоматически)
- WallpaperFolder - Папка с обоями, которые будут загружены в этот каталог (значение по умолчанию: "C:\Users\Public\Pictures\Wallpapers")
- WallpaperFileHistory - Путь к файлу, где хранится список загруженных изображений, необходим для того, чтобы не загружать ранее загруженные изображения. (значение по умолчанию: "C:\Users\Public\Pictures\image_list.json")

## Dобавить программу в автозагрузку

### Window 10

1. Делаем ярлык программы
   1. ПКМ по `Wallpaper.exe` => `Отправить` => `На рабочий стол`
2. В адресную строку проводника вставте %AppData%\Microsoft\Windows\Start Menu\Programs\Startup и нажмите клавишу Enter, или в меню “Выполнить” (выполнить вызывается клавишами Win+R) введите  shell:startup и нажмите Enter.
3. Откроется папка автозагрузки, копируйте сюда ярлык


## Настроить ОС для автоматической смены изображений рабочего стола

https://www.endsight.net/blog/how-to-create-a-custom-windows-10-slideshow-background-lock-screen
