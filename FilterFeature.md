# Функционал фильтрации для BlazorTable
В этой статье мы подробно обсудим, как создать фильтр для таблицы, разработка которого описана [в статье по ссылке](https://github.com/Amangeldi/BlazorTable),
рассмотрим то как рефлексия поможет нам в выявлении свойств объекта, избавимся от наших RenderFragment-ов и обновим наш пакет в системе управления пакетами nuget.
## Что нового в компоненте BlazorTable?
Скачайте решение BlazorTable   [СКАЧАТЬ](https://github.com/Amangeldi/BlazorTable)
Идея о том, что в TableHeader нужно передавать заголовки колонок, а в TableRow передавать свойства показалось мне не самой лучшей. Было принято решение что лучше в разметке вместо
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
пользователю будет гораздо легче использовать компоненту следующим образом:
```c#
<Table Items="@forecasts" PageSize="4">
</Table>
```
Для начало избавимся от параметров TableHeader
```c#
[Parameter]
public RenderFragment TableHeader { get; set; }
[Parameter]
public RenderFragment<TItem> TableRow { get; set; }
```
## Реализуем фильтр
Для хранения делегата фильтров будем использовать коллекцию кортежей
```c#
public List<(string, Func<TItem, string>, string)> filters = new List<(string, Func<TItem, string>, string)>();
```
Где первый параметр string будет хранить имя свойства, второй делегат класса в виде p =>p.Property.ToString(), третий для хранения вводимых значений. <br>
Для отображения вместо параметра Items будем использовать свойсво DisplayedItems
```c# 
        public IEnumerable<TItem> DisplayedItems { get
            { 
                if(filters.Count()==0)
                {
                    return Items;
                }
                else
                {
                    return FilteredItems;
                }
            } 
        }
```
При инициализации компоненты мы получаем коллекцию свойств для TItem.
```c#
        protected override void OnInitialized()
        {
            properties = typeof(TItem).GetProperties();
            ...
        }
```
В начале таблицы отображаем input-ы равные количеству свойств, и при клике каждого вызывается метод ApplyFilter который принимает PropertyInfo и текст введенный в input.
```c#
            @foreach (var property in properties)
            {
                <td>
                    <input type="text" class="form-control" @oninput="e=>ApplyFilter(property, e.Value.ToString())" />
                </td>
            }
```
В методе ApplyFilter нам понадобится достать делегат Func<T, string> из значения параметра PropertyInfo
```c#
        public static Func<T, string> GetPropertyDelegate<T>(PropertyInfo property)
        {
            return x => property.GetValue(x)?.ToString();
        }
```
Делегаты - это мощный инструмент, но сдесь они излишний функционал. Используя их я столкнулся с проблемой сравнивания делегатов типа p=>p.Property. predicate.Equals(func) не будет работать, так как во время исполнения программы они будут представлены разными анонимными методами. Конечно же есть способ сравнить их по IL коду:
```c#
    static bool AreMethodsEqual(MethodBase left, MethodBase right)
    {
        MethodBody m1 = left.GetMethodBody();
        MethodBody m2 = right.GetMethodBody();
        byte[] il1 = m1.GetILAsByteArray();
        byte[] il2 = m2.GetILAsByteArray();
        return il1.SequenceEqual(il2);
    }
```
Если нет другого способа сравнить делегаты Func, то это решение работает. Но в 99% случаях есть решение проще и правильнее.
В нашем случае мы можем убрать с нашего кортежа значение Func и использовать метод GetValue:
```c#
            foreach(var filter in filters)
            {
                FilteredItems = FilteredItems.Where(p=>filter.Item1.GetValue(p).ToString().Contains(filter.Item2)).ToList();
            }
```
И для отображения данных мы воспользуемся property.GetValue(item)
```c#
        @foreach (var item in ItemList)
        {
            <tr>
                @foreach (var property in properties)
                {
                    <td>
                        @if (property.PropertyType == typeof(DateTime))
                        {
                            @(((DateTime)property.GetValue(item)).ToShortDateString())
                        }
                        else
                        {
                            @property.GetValue(item)
                        }
                    </td>
                }
            </tr>
        }
```
## Листинг Table.razor.cs
```c#
public class TableBase<TItem> : ComponentBase
    {
        public const int PAGER_SIZE = 6;
        public int pagesCount;
        public int curPage;
        public int startPage;
        public int endPage;
        [Parameter]
        public IEnumerable<TItem> Items { get; set; }
        [Parameter]
        public int PageSize { get; set; }
        public IEnumerable<TItem> ItemList { get; set; }
        List<TItem> filteredItems = new List<TItem>();
        public List<TItem> FilteredItems { 
            get
            { 
                if(filteredItems.Count()==0)
                {
                    return Items.ToList();
                }
                else
                {
                    return filteredItems;
                }
            }
            set 
            {
                filteredItems = value;
            } 
        } 
        public IEnumerable<TItem> DisplayedItems { get
            { 
                if(filters.Count()==0)
                {
                    return Items;
                }
                else
                {
                    return FilteredItems;
                }
            } 
        }
        public PropertyInfo[] properties;
        public List<(PropertyInfo, string)> filters = new List<(PropertyInfo, string)>();
        protected override void OnInitialized()
        {
            properties = typeof(TItem).GetProperties();
            Refresh();
        }
        public void Refresh()
        {
            curPage = 1;
            ItemList = DisplayedItems.Skip((curPage - 1) * PageSize).Take(PageSize);
            pagesCount = (int)Math.Ceiling(DisplayedItems.Count() / (decimal)PageSize);
            SetPagerSize("forward");
            StateHasChanged();
        }
        public void ApplyFilter(PropertyInfo property, string text)
        {
            if (filters.Where(p => p.Item1 == property).Count() > 0 && !string.IsNullOrEmpty(text))
            {
                var filter = filters.Where(p => p.Item1 == property).First();
                filters.Remove(filter);
                filter.Item2 = text;
                filters.Add(filter);
            }
            else if(filters.Where(p => p.Item1 == property).Count() > 0 && string.IsNullOrEmpty(text))
            {
                var filter = filters.Where(p => p.Item1 == property).First();
                filters.Remove(filter);
            }
            else if(!(filters.Where(p => p.Item1 == property).Count() > 0) && !string.IsNullOrEmpty(text))
            {
                (PropertyInfo, string) filter = (property, text);
                filters.Add(filter);
            }
            foreach(var filter in filters)
            {
                FilteredItems = FilteredItems.Where(p=>filter.Item1.GetValue(p).ToString().Contains(filter.Item2)).ToList();
            }
            Refresh();
        }
        public void UpdateList(int currentPage)
        {
            ItemList = DisplayedItems.Skip((currentPage - 1) * PageSize).Take(PageSize);
            curPage = currentPage;
            StateHasChanged();
        }
        public void SetPagerSize(string direction)
        {
            if (direction == "forward" && endPage < pagesCount)
            {
                startPage = endPage + 1;
                if (endPage + PAGER_SIZE < pagesCount)
                {
                    endPage = startPage + PAGER_SIZE - 1;
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
                startPage = startPage - PAGER_SIZE;
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
        public static Func<T, string> GetPropertyDelegate<T>(PropertyInfo property)
        {
            return x => property.GetValue(x)?.ToString();
        }
    }
```
## Итого
![alt text](https://github.com/Amangeldi/BlazorTable/blob/master/imgs/filter.png?raw=true)
Мы реализовали функционал фильтрации для нашей компоненты BlazorTable. Он доступен по адресу https://www.nuget.org/packages/AmanTable/1.2.0. Процесс реализации самой таблицы можете прочитать [тут](https://github.com/Amangeldi/BlazorTable/blob/master/How%20to%20create%20a%20table%20blazor%20component.md)
