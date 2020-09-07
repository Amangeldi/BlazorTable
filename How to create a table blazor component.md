# BlazorTable - компонент таблица многоразового использования для Blazor
В этом блоге мы обсудим как с нуля разработать компоненту Blazor и упакуем его в системе управления пакетами nuget. Он нам понадобиться для отображения пользовательских данных.
Что нам для этого понадобиться
* Visual Studio последняя версия [Скачать](https://visualstudio.microsoft.com/ru/downloads/)
* Bootstrap версии 4.4 [Скачать](https://getbootstrap.com/docs/4.4/getting-started/introduction/)
## Создание библиотеки Blazor
Чтобы создать проект библиотеки blazor, выполните действия указанные по [ссылке](https://docs.microsoft.com/ru-ru/aspnet/core/blazor/components/class-libraries?view=aspnetcore-3.1&tabs=visual-studio#create-an-rcl)
Будет создан проект библиотеки Blazor, со структурой файлов как указано ниже <br>
![screenshot of sample](https://github.com/Amangeldi/BlazorTable/blob/master/imgs/structure.png?raw=true) <br>
В этом проекте есть несколько предопределенных файлов. Мы можем удалить их всех, кроме styles.css позже мы в него поместим стили для нашего компонента.  <br>
Теперь мы добавим в наш проект компоненту Table. <br>
Щелкните правой кнопкой мыши на ваш проект и выберите добавить>> Создать элемент. Откроется диалоговое окно «Добавить новый элемент», выберите «Компонент Razor» на панели шаблонов и назовите его Table.razor  <br>
![screenshot of sample](https://github.com/Amangeldi/BlazorTable/blob/master/imgs/CreateComponent.png?raw=true) <br>
Так же для разделения кода от разметки создадим класс Table.razor.cs
Visual studio сгруппирует наш файл Table.razor и Table.razor.cs  <br>
![screenshot of sample](https://github.com/Amangeldi/BlazorTable/blob/master/imgs/ComponentCode.png?raw=true) <br>
Переименуем класс кода в TableBase с универсальным параметром TItem и опишем в ней следующий код:
```c#
    public class TableBase<TItem>:ComponentBase
    {
        public int pagesCount;
        public int curPage;
        public int pagerSize;
        public int startPage;
        public int endPage;
        [Parameter]
        public RenderFragment TableHeader { get; set; }
        [Parameter]
        public RenderFragment<TItem> TableRow { get; set; }
        [Parameter]
        public IEnumerable<TItem> Items { get; set; }
        [Parameter]
        public int PageSize { get; set; }
        public IEnumerable<TItem> ItemList { get; set; }
        protected override async Task OnInitializedAsync()
        {
            pagerSize = 6;
            curPage = 1;
            ItemList = Items.Skip((curPage - 1) * PageSize).Take(PageSize);
            pagesCount = (int)Math.Ceiling(Items.Count() / (decimal)PageSize);
            SetPagerSize("forward");
        }
        public void UpdateList(int currentPage)
        {
            ItemList = Items.Skip((currentPage - 1) * PageSize).Take(PageSize);
            curPage = currentPage;
            StateHasChanged();
        }
        public void SetPagerSize(string direction)
        {
            if (direction == "forward" && endPage < pagesCount)
            {
                startPage = endPage + 1;
                if (endPage + pagerSize < pagesCount)
                {
                    endPage = startPage + pagerSize - 1;
                }
                else
                {
                    endPage = pagesCount;
                }
                StateHasChanged();
            }
            else if (direction == "back" && startPage > 1)
            {
                endPage = startPage - 1;
                startPage = startPage - pagerSize;
            }
        }
        public void NavigateToPage(string direction)
        {
            if (direction == "next")
            {
                if (curPage < pagesCount)
                {
                    if (curPage == endPage)
                    {
                        SetPagerSize("forward");
                    }
                    curPage += 1;
                }
            }
            else if (direction == "previous")
            {
                if (curPage > 1)
                {
                    if (curPage == startPage)
                    {
                        SetPagerSize("back");
                    }
                    curPage -= 1;
                }
            }
            UpdateList(curPage);
        }
    }
```
Определим 4 параметра для нашего компонента:
* TableHeader : заголовок для Table.
* TableRow : строки для Table.
* Items : список элементов, переданных в Table.
* PageSize : размер каждой страницы Table.
В компоненте Table.razor отметим следующий код:
```html
@inherits TableBase<TItem>
@typeparam TItem
<table class="table table-striped">
    <thead>
        <tr class="">@TableHeader</tr>
    </thead>
    <tbody>
        @foreach (var item in ItemList)
        {
            <tr class="">@TableRow(item)</tr>
        }
    </tbody>
</table>
<div class="pagination">
    <button class="btn btn-info" @onclick=@(async () => SetPagerSize("back"))>&laquo;</button>
    <button class="btn btn-secondary" @onclick=@(async () => NavigateToPage("previous"))>Пред.</button>
    @for (int i = startPage; i <= endPage; i++)
    {
        var currentPage = i;
        <button class="btn @(currentPage==curPage?"currentpage":"")" @onclick=@(async () => UpdateList(currentPage))>
            @currentPage
        </button>
    }
    <button class="btn btn-secondary" @onclick=@(async () => NavigateToPage("next"))>След.</button>
    <button class="btn btn-info" @onclick=@(async () => SetPagerSize("forward"))>&raquo;</button>
    <span class="btn btn-link disabled">Page @curPage of @pagesCount</span>
</div>
<head>
    <link rel="stylesheet" type="text/css" href="/styles.css">
    <link rel="stylesheet" type="text/css" href="/bootstrap/bootstrap.min.css">
</head>
```
Директива @inherits используется для указания базового класса. <br>
Директива @typeparam используется для указания универсального компонента. <br>
В переопределенном методе OnInitializedAsync мы инициализируем pagerSize равным пяти и устанавливаем curPage равным единице <br>
Метод UpdateList вызывается, когда мы нажимаем на кнопку страницы. Он объявляет коллекцию ItemList. <br>
Метод SetPagerSize будет устанавливать номер страницы в каждом наборе пагинатора. Работу пагинатора можно проиллюстрировать так: <br>
![screenshot of sample](https://github.com/Amangeldi/BlazorTable/blob/master/imgs/%D0%BF%D0%B0%D0%B3%D0%B8%D0%BD%D0%B0%D1%82%D0%BE%D1%80.gif?raw=true) <br>
Метод NavigateToPage вызывается по нажатию кнопки След или Пред. Он будет перемещать пользователя непосредственно к следующей или непосредственно к предыдущей странице <br>
## Добавление CSS стилей для компоненты.
Добавьте bootstrap 4.4 стили в папку wwwroot/bootstrap и подключите их в теге head в файле разметки компоненты. Вы можете дополнить их собственными стилями в файле styles.css:
```css
.currentpage {
    background-color: dodgerblue;
    color: white;
}
```
## Добавление компоненты Table в проект Blazor
Создайте новый шаблонный проект blazor и добавьте ссылку на нашу библиотеку.
Откройте файл FetchData.razor и определите следующий код в разделе else
```html
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
## Демонстрация исполнения
Запустите приложение и перейдите на страницу получения данных. Вы увидите, что данные отображаются в виде сетки, как показано на изображении ниже: <br>
![screenshot of sample](https://github.com/Amangeldi/BlazorTable/blob/master/imgs/1.png?raw=true) <br>
# Размещение компоненты в менеджере пакетов NuGet
Для размещения компоненты, выполните следующие действия:
* Зарегистрируйтесь на сайте nuget.org
*	В Visual Studio перейдите в меню Проект >> Свойства >> Пакет и поставьте галочку в «Формировать NuGet при сборке»:
![screenshot of sample](https://github.com/Amangeldi/BlazorTable/blob/master/imgs/NuGet.png?raw=true) <br>
* Выполните сборку проекта. В директории /bin/debug/ появится пакет BlazorTable.1.0.0.nupkg
* На сайте nuget.org перейдите на вкладку Manage Packages. И загрузите собранный файл пакетов.
* Следуйте инструкциям на сайте.
# Вывод
Мы создали общий компонент Blazor - Table. Он отображает пользовательские данные в таблице. Этот компонент также обеспечивает пагинацию на стороне клиента. Мы узнали, как ссылаться на общий компонент и использовать его в приложении Blazor.<br>
Получите исходный код с [GitHub](https://github.com/Amangeldi/BlazorTable) и поэкспериментируйте, чтобы лучше понять.
