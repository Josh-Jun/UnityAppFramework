/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月8 15:16
 * function    :
 * ===============================================
 * */

using System.ComponentModel;

public delegate void SROptionsPropertyChanged(object sender, string propertyName);

public partial class GM : INotifyPropertyChanged
{
    private static GM _current;
    public static GM Current => _current ??= new GM();

    public event SROptionsPropertyChanged PropertyChanged;
    
#if UNITY_EDITOR
    [JetBrains.Annotations.NotifyPropertyChangedInvocator]
#endif
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, propertyName);

        InterfacePropertyChangedEventHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private event PropertyChangedEventHandler InterfacePropertyChangedEventHandler;

    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add => InterfacePropertyChangedEventHandler += value;
        remove => InterfacePropertyChangedEventHandler -= value;
    }
}