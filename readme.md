Oven
====

![bakeware](img/bakeware.png) + ![bread_filling](img/bread_filling.png)
<br><br>

인터페이스와 구현체를 런타임에 바인딩하여 인스턴스를 찍어냅니다.

`dynamic` 키워드와의 차이점
----
* `dynamic` 키워드는 다이나믹 바인딩을 위해 컴파일 타임 타입, 네이밍 검사를 포기합니다. 하지만 __Oven__은 미리 정의된 인터페이스에 다이나믹한 구현체를 바인딩 할 수 있기 때문에 컴파일 타임에 엄격하게 체크됩니다.

빵 굽는 법
----
__빵틀__을 준비합니다. 빵틀은 찍어낼 오브젝트의 인터페이스입니다.
```c#
public interface Math
{
  int Sum(int a,int b);
}
```
__빵에 넣을 재료__를 만듭니다. 
```c#
public class MathImpl : IFilling
{
  public object OnMethod(Type type, MethodInfo method, object[] args)
  {
    if(method.Name == nameof(Bread.Sum))
      return (int)args[0] + (int)args[1];
    else
      throw new NotImplementedException();
  }
}
```
빵틀과 재료를 혼합하여 빵을 굽습니다.
```c#
var bread = Oven.Bake<Math, MathImpl>();

Console.WriteLine(bread.Sum(1, 2));
```



Oven
====

![bakeware](img/bakeware.png) + ![bread_filling](img/bread_filling.png)
<br><br>

Implement interfaces at the RUNTIME.

Difference between `dynamic` and `Oven`
----
* __Oven__ : Strict type and name checking in compile time.
* __Dynamic__ : 
* __Common__ :  Dynamic linking between interface and implementation

How to Bake
----
Prepare a `Bakeware`
```c#
public interface Math
{
  int Sum(int a,int b);
}
```
Make `Fillings`
```c#
public class MathImpl : IFilling
{
  public object OnMethod(Type type, MethodInfo method, object[] args)
  {
    if(method.Name == nameof(Bread.Sum))
      return (int)args[0] + (int)args[1];
    else
      throw new NotImplementedException();
  }
}
```
Time to bake now
```c#
var bread = Oven.Bake<Math, MathImpl>();

Console.WriteLine(bread.Sum(1, 2));
```
