   M S B A M L   `   `   `  ����A  =WpfApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:WpfAppWpfApp  5   	       WpfApp.Account_Login   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:WpfApp     5   	     � Title$  Account Login��5   	   $	��271��6   $	��320��6,    � WindowStartupLocation$ CenterScreen=�68   
������  � WindowStyle$
 None=�6v    � 
ResizeMode$ NoResize=�6�    � AllowsTransparency . .��5	      � .+�5
      �� $	��320��6   $	��271��6   
������� �� RadiusX$ 20��64    �� RadiusY$ 20��6A   �� 5      $��	Loginname��6
   $
��Left=�6   $��	10,10,0,0q�69   $	��Top=�6L   �� 5      -   5        $��txtb_loginalias��5      $	��300��6#   $��38��6/   $��10,36,10,197q�6;   $��1q�6Q   
���   � �� 	MaxLength$ 50��6y    �� MaxLines$ 1��6�   $��20(�6�   
	 � Tag$	 Loginnamen eingeben...��6�   �� 5      $��Passwort��6
   $
��Left=�6   $��	10,83,0,0q�68   $	��Top=�6K   G� 5      -   5        $��pwb_loginpassword��5      $	��300��6)   $��38��65   $��10,109,10,124q�6A   $��1q�6X   
���   �
 G� 	MaxLength$
 50��6�   $��20(�6�   $	 Passwort eingeben...��6�   �� 5      -   5        $��lbl_warning��5      $�� ��6!   $��Stretch=�6)   $��10,175,10,0q�6G   
���  ��$��42��6m   $��Disabled=�6y   $��WrapWithOverflow=�6�   $	��Top=�6�   �� 5      -   5        $��	Btn_login��5      $��Login��6   $
��Left=�6,   $��
10,222,0,0q�6G   $	��Top=�6[   $	��300��6s   $��38��6   $��18(�6�   5      5      