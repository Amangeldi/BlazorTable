# BlazorTable
Это многоразовый компонент таблицы для Blazor. Он также поддерживает пагинацию на стороне клиента.
Компонент BlazorTable можно использовать только в приложении Blazor. Чтобы создать приложение Blazor, следуйте инструкциям на https://docs.microsoft.com/ru-ru/aspnet/core/tutorials/build-a-blazor-app?view=aspnetcore-3.1
# Менеджер пакетов NuGet
Страницу пакета Nuget можно найти по адресу https://www.nuget.org/packages/AmanTable/
# Методы скачивания пакета
Чтобы установить AmanTable с помощью NPM, выполните следующую команду
```
Install-Package AmanTable
```
Чтобы установить AmanTable с помощью .NET CLI, выполните следующую команду
```
dotnet add package AmanTable
```
После установки пакета добавьте в _Imports.razor файл следующую строку
```C#
@using BlazorTable
```
# Пример использования
Версия 1.1.0
```c#
<Table Items="@forecasts" PageSize="4">
        <TableHeader>
            <th>Date</th>
            <th>TemperatureC</th>
            <th>TemperatureF</th>
            <th>Summary</th>
        </TableHeader>
        <TableRow>
            <td>@context.Date.ToShortDateString()</td>
            <td>@context.TemperatureC</td>
            <td>@context.TemperatureF</td>
            <td>@context.Summary</td>
        </TableRow>
    </Table>
```
Версия 1.2.0
```c#
<Table Items="@forecasts" PageSize="4">
</Table>
```
# Пример вывода
![alt text](https://raw.githubusercontent.com/Amangeldi/BlazorTable/master/1.png)
Процесс создания компоненты описан в статье по [ссылке](https://github.com/Amangeldi/BlazorTable/blob/master/How%20to%20create%20a%20table%20blazor%20component.md)
# Что нового в версии 1.2.0
Добавил функционал фильтрации <br>
избавился от RenderFragment-ов в пользу рефлексии
# Обратная связь
Не стесняйтесь использовать этот компонент и оставлять свои ценные отзывы. Если вы столкнетесь с ошибками, откройте Issue и обсудите его.
