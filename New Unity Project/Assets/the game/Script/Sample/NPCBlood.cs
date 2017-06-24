using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBlood : MonoBehaviour
{
    //主摄像机对象	
    private Camera ccc;
    //NPC名称	
    private new string name = "test";
    //主角对象	
    GameObject hero;
    //NPC模型高度	
    float npcHeight;
    float npcWidth;
    //红色血条贴图	
    public Texture2D blood_red;
    //黑色血条贴图	
    public Texture2D blood_black;
    //默认NPC血值	
    private int HP = 100;
    public int del = 0;
    void Start()
    {
        //根据Tag得到主角对象		
        hero = GameObject.FindGameObjectWithTag("Player");
        //得到摄像机对象		
        ccc = Camera.main;
        //注解1		
        //得到模型原始高度		
        float size_y = GetComponent<Collider>().bounds.size.y;
        float size_x = GetComponent<Collider>().bounds.size.x;
        //    collider.bounds.size.y;
        //得到模型缩放比例		
        float scal_y = transform.localScale.y;
        float scal_x = transform.localScale.x;
        //它们的乘积就是高度		
        npcHeight = (size_y * scal_y);
        npcWidth = (size_x * scal_x);

        // 设置texture2d的大小以及颜色
        blood_red = new Texture2D(10, 10, TextureFormat.ARGB32, false);
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
            {
                blood_red.SetPixel(i, j, new Color(1.0f, 0, 0, 1));
            }
        blood_red.Apply(false);

        blood_black = new Texture2D(10, 10, TextureFormat.ARGB32, false);
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
            {
                blood_black.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        blood_black.Apply(false);
    }
    void Update()
    {
        HP = GetComponent<Unit>().HP;
        
        //保持NPC一直面朝主角		
        transform.LookAt(hero.transform);
    }
    void OnGUI()
    {

        if (HP <= 0)
        {
            return;
        }
        Vector3 worldPosition = new Vector3(transform.position.x, transform.position.y + npcHeight, transform.position.z);
        Vector3 L = new Vector3(transform.position.x, transform.position.y, transform.position.z - npcWidth);
        Vector3 R = new Vector3(transform.position.x, transform.position.y, transform.position.z + npcWidth);
        Vector2 position = ccc.WorldToScreenPoint(worldPosition);
        Vector2 PL = ccc.WorldToScreenPoint(L);
        Vector2 PR = ccc.WorldToScreenPoint(R);
        Vector2 Dis = PL - PR;	
        position = new Vector2(position.x, Screen.height - position.y);
        Vector2 bloodSize = new Vector2(Dis.y, 5);

        // 固定血条大小
        bloodSize = new Vector2(40, 10);
        // 黑色为底，红色为剩余血条
        int blood_width = (int)(bloodSize.x * (HP) / 100);
        GUI.DrawTexture(new Rect(position.x - (bloodSize.x / 2), position.y - bloodSize.y-del, bloodSize.x, bloodSize.y), blood_black);
        GUI.DrawTexture(new Rect(position.x - (bloodSize.x / 2), position.y - bloodSize.y-del, blood_width, bloodSize.y), blood_red);
        Vector2 nameSize = new Vector2(25, 25);
        GUI.color = Color.yellow;
        GUI.Label(new Rect(position.x - (nameSize.x / 2), position.y - nameSize.y - bloodSize.y, nameSize.x, nameSize.y), name);
    }
}
