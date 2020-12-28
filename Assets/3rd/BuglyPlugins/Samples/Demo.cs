using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

//using BuglyUnity;

public class Demo : MonoBehaviour
{
    private int selGridIntCurrent = 5;
    private int selGridIntDefault = -1;
    private Vector2 scrollPosition = Vector2.zero;

    private string[] selGridItems = new string[] {"Exception","SystemException","ApplicationException",
        "ArgumentException","FormatException","...",
				"MemberAccessException","FileAccessException","MethodAccessException","MissingMemberException","MissingMethodException","MissingFieldException",
        "IndexOutOfException","ArrayTypeMismatchException","RankException",
        "IOException","DirectionNotFoundException","FileNotFoundException","EndOfStreamException","FileLoadException","PathTooLongException",
        "ArithmeticException","NotFiniteNumberException","DivideByZeroException",
        "OutOfMemoryException","NullReferenceException","InvalidCastException","InvalidOperationException",
        "",""
    };

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID
        //当用户按下手机的返回键或home键退出游戏
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
        {
            Application.Quit();
        }
#endif
    }

    void OnGUI()
    {

        // set the base area
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

        GUILayout.BeginVertical();

        // set the title bar
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Bugly Unity Demo");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // set layout
        GUILayout.BeginVertical();
        GUILayout.Label("Uncaught Exceptions:");
        GUILayout.Space(20);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height - 100));

        //      GUILayout.BeginArea (new Rect(20,100, Screen.width - 20, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        selGridIntCurrent = GUILayout.SelectionGrid(selGridIntCurrent, selGridItems, 3);
        GUILayout.Space(20);
        GUILayout.EndHorizontal();
        //              GUILayout.EndArea ();
        GUILayout.EndScrollView();

        if (selGridIntCurrent != selGridIntDefault)
        {
            selGridIntDefault = selGridIntCurrent;

            TrigException(selGridIntCurrent);
        }

        GUILayout.EndVertical();

        GUILayout.EndVertical();
        GUILayout.EndArea();

    }

    private void TrigException(int selGridInt)
    {

        switch (selGridInt)
        {
            case 0:
                throwException(new System.Exception("Non-fatal error, an base C# exception"));
                break;
            case 1:
                throwException(new System.SystemException("Fatal error, a system exception"));
                break;
            case 2:
                throwException(new System.ApplicationException("Fatal error, an application exception"));
                break;
            case 3:
                throwException(new System.ArgumentException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 4:
                throwException(new System.FormatException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 5: // ignore
                break;
            case 6:
                throwException(new System.MemberAccessException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 7:
                throwException(new System.FieldAccessException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 8:
                throwException(new System.MethodAccessException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 9:
                throwException(new System.MissingMemberException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 10:
                throwException(new System.MissingMethodException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 11:
                throwException(new System.MissingFieldException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 12:
                throwException(new System.IndexOutOfRangeException(string.Format("Non-Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 13:
                throwException(new System.ArrayTypeMismatchException(string.Format("Non-Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 14:
                throwException(new System.RankException(string.Format("Non-Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
                try
                {
                    throwException(new System.Exception(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                }
                catch (System.Exception e)
                {
                    BuglyAgent.ReportException(e, "Caught an exception.");
                }
                break;
            case 21:
                throwException(new System.ArithmeticException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 22:
                throwException(new System.NotFiniteNumberException(string.Format("Fatal error, {0} ", selGridItems[selGridInt])));
                break;
            case 23:
                int i = 0;
                i = 2 / i;
                break;
            case 24:
                throwException(new System.OutOfMemoryException("Fatal error, OOM"));
                break;
            case 25:
                findGameObject();
                break;
            case 26:
                System.Exception excep = null;
                System.IndexOutOfRangeException iore = (System.IndexOutOfRangeException)excep;
                System.Console.Write("" + iore);
                break;
            case 27:
                findGameObjectByTag();
                break;
            case 28:
                DoCrash();
                break;
            case 29:
            default:
                try
                {
                    throwException(new System.OutOfMemoryException("Fatal error, out of memory"));
                }
                catch (System.Exception e)
                {
                    BuglyAgent.ReportException(e, "Caught Exception");
                }
                break;
        }

    }

    private void findGameObjectByTag()
    {
        System.Console.Write("it will throw UnityException");
        GameObject go = GameObject.FindWithTag("test");

        string gName = go.name;
        System.Console.Write(gName);
    }

    private void findGameObject()
    {
        System.Console.Write("it will throw NullReferenceException");

        GameObject go = GameObject.Find("test");
        string gName = go.name;

        System.Console.Write(gName);
    }

    private void throwException(Exception e)
    {
        if (e == null)
            return;


        testDeepFrame(e);
    }

    private void testDeepFrame(Exception e)
    {

        throw e;
    }

    private void DoCrash()
    {
        System.Console.Write("it will Crash...");

        string[] strs = new string[100000000];
#pragma warning disable 0219
        int len = strs[0].Length;
#pragma warning restore 0219
        DoCrash();
    }

}
