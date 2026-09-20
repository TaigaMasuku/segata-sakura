using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace SaturnApp {
// Native Windows Common Item Dialog: Explorer navigation, address bar and search.
internal static class FolderPicker {
 [ComImport,Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")] class FileOpenDialog {}
 [ComImport,Guid("42f85136-db7e-439c-85f1-e4075d135fc8"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
 interface IFileDialog {
  [PreserveSig] int Show(IntPtr parent);
  void SetFileTypes(uint count,IntPtr specs); void SetFileTypeIndex(uint index); void GetFileTypeIndex(out uint index);
  void Advise(IntPtr events,out uint cookie); void Unadvise(uint cookie);
  void SetOptions(uint options);void GetOptions(out uint options);
  void SetDefaultFolder(IShellItem folder);void SetFolder(IShellItem folder);void GetFolder(out IShellItem folder);
  void GetCurrentSelection(out IShellItem item);void SetFileName([MarshalAs(UnmanagedType.LPWStr)]string name);
  void GetFileName([MarshalAs(UnmanagedType.LPWStr)]out string name);void SetTitle([MarshalAs(UnmanagedType.LPWStr)]string title);
  void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)]string text);void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)]string text);
  void GetResult(out IShellItem item);
 }
 [ComImport,Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
 interface IShellItem {
  void BindToHandler(IntPtr bind,ref Guid handler,ref Guid iid,out IntPtr result);
  void GetParent(out IShellItem parent);void GetDisplayName(uint kind,out IntPtr name);
  void GetAttributes(uint mask,out uint attributes);void Compare(IShellItem other,uint hint,out int order);
 }
 public static string Choose(IWin32Window owner) {
  var dialog=(IFileDialog)new FileOpenDialog();IShellItem item=null;IntPtr name=IntPtr.Zero;
  try {uint options;dialog.GetOptions(out options);dialog.SetOptions(options|0x20u|0x40u|0x800u);dialog.SetTitle("Choisir le dossier de vos jeux Saturn");dialog.SetOkButtonLabel("Ajouter ce dossier");
   int result=dialog.Show(owner.Handle);if(result==unchecked((int)0x800704C7))return null;Marshal.ThrowExceptionForHR(result);
   dialog.GetResult(out item);item.GetDisplayName(0x80058000,out name);return Marshal.PtrToStringUni(name);
  } finally {if(name!=IntPtr.Zero)Marshal.FreeCoTaskMem(name);if(item!=null)Marshal.ReleaseComObject(item);Marshal.ReleaseComObject(dialog);}
 }
}
}
