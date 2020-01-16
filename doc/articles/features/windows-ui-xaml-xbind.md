# Uno Support for x:Bind

Uno supports the [`x:Bind`](https://docs.microsoft.com/en-us/windows/uwp/xaml-platform/x-bind-markup-extension) WinUI feature, which gives the ability the bind to normal fields and properties, static classes fields, and functions with multiple parameters.

# Examples
- Properties
  - Page or control property
  ```xaml
  <TextBlock Text="{x:Bind MyProperty}" />
  ```
  - Member function
  ```xaml
  <TextBlock Text="{x:Bind MyProperty.ToUpper()}" />
  ```
  - Static types field or properties OneTime binding
  ```xaml
  <TextBlock Text="{x:Bind local:StaticType.PropertyIntValue}" />
  ```
  - OneWay local member function with multiple observable parameters
  ```xaml
  <TextBlock Text="{x:Bind Multiply(slider1.Value, slider2.Value), Mode=OneWay}" />
  ```
  - OneWay static class function with  multiple observable parameters
  ```xaml
  <TextBlock Text="{x:Bind local:StaticType.Add(slider1.Value, slider2.Value), Mode=OneWay}" />
  ```
  - Literal boolean parameters (`x:True`, `x:False`)
  ```xaml
  <TextBlock Text="{x:Bind BoolFunc(x:False)}" />
  ```
  - Null parameter (`x:Null`)
  ```xaml
  <TextBlock Text="{x:Bind TestString(x:Null)}" />
  ```
  - Quote escaping
  ```xaml
  <TextBlock Text="{x:Bind sys:String.Format('{0}, ^'{1}^'', InstanceProperty, StaticProperty)}" />
  ```
  - Literal numeric value
  ```
  <TextBlock Text="{x:Bind Add(InstanceProperty, 42.42)}" />
  ```

- Use of system functions (given `xmlns:sys="using:System"`):
  - Single parameter formatting:
    ```xaml
    <TextBlock Text="{x:Bind sys:String.Format('Formatted {0}', MyProperty), Mode=OneWay}" />
    ```
  - Multi parameters formatting:
    ```xaml
    <TextBlock Text="{x:Bind sys:String.Format(x:Null, 'slider1: {0}, slider2:{1}', slider1.Value, slider2.Value), Mode=OneWay}" />
    ```
  - TimeParsing:
    ```xaml
    <CalendarDatePicker Date="{x:Bind sys:DateTime.Parse(TextBlock1.Text)}" />
    ```

# Not supported
- BindBack: `<TextBlock Text="{x:Bind a.MyFunc(b), BindBack=a.MyFunc2, Mode=TwoWay}" />`
- Type casts 