
using UnityEngine;
using System.Collections;

public class SampleGui : MonoBehaviour 
{

	private GameController game;
    //预设坐标位置
	private Rect winRect = new Rect(10f, 10f, 300f, 100f);

    void Start()
	{
		game = gameObject.GetComponent<GameController>();
	}

	void OnGUI()
	{
        if (game.allowInput)
		{
            //创建外部框用于设定框的类型
			winRect = GUILayout.Window(0, winRect, theWindow, "Game");
		}
    }

	private void theWindow(int id)
	{
        //与上方元素(即外部框)间隔10个单位
		GUILayout.Space(10f);
        //增加文本内容
		game.useTurns = GUILayout.Toggle(game.useTurns, "USE TURNS");

        if (game.useTurns)
		{
			GUILayout.Space(10f);
            //设置字体大小
            GUI.skin.button.fontSize = 80;
            //当点击按钮时，切换Turn
            if (GUILayout.Button("Change Turns")) 
            {
                game.ChangeTurn();
            }

			GUILayout.Space(10f);
			GUILayout.Label(string.Format("Player {0}'s Turn", game.currPlayerTurn+1));
		}
	}
}
