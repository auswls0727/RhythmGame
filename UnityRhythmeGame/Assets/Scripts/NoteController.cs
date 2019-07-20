﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NoteController : MonoBehaviour
{
    // Start is called before the first frame update

    //하나의 노트에 대한 정보를 담는 노트(Note) 클래스 정의
    class Note{
        public int noteType { get; set; }
        public int order { get; set; }
        public Note(int noteType, int order){
            this.noteType = noteType;
            this.order = order;
        }
    }

    public GameObject[] Notes;
    private ObjectPooler noteObjectPooler;
    private List<Note> notes = new List<Note>();
    private float x, z, startY = 8.0f;

    void MakeNote(Note note){
        GameObject obj = noteObjectPooler.getObject(note.noteType);

        //설정된 시작 라인으로 노트 이동
        x = obj.transform.position.x;
        z = obj.transform.position.z;
        obj.transform.position = new Vector3(x, startY, z);
        obj.GetComponent<NoteBehavior>().Initialize();
        obj.SetActive(true);
    }

    private string musicTitle;
    private string musicArtist;
    private int bpm;
    private int divider;
    private float startingPoint;
    private float beatCount;
    private float beatInterval;

    IEnumerator AwaiteMakeNote(Note note){
        int noteType = note.noteType;
        int order = note.order;
        yield return new WaitForSeconds(startingPoint + order * beatInterval);
        MakeNote(note);
    }
    void Start()
    {
        noteObjectPooler = gameObject.GetComponent<ObjectPooler>();

        //리소스에서 비트 텍스트 파일 불러오기
        TextAsset textAsset = Resources.Load<TextAsset>("Beats/" + GameManager.instance.music);
        StringReader reader = new StringReader(textAsset.text);

        //첫번째 줄에 적힌 곡 이름 읽기
        musicTitle = reader.ReadLine();
        //두번째 줄에 적힌 아티스트 이름 읽기
        musicArtist = reader.ReadLine();
        //세번째 줄에 적힌 비트 정보(bpm, divider, 시작시간) 읽기
        string beatInformation = reader.ReadLine();
        bpm = Convert.ToInt32(beatInformation.Split(' ')[0]);
        divider = Convert.ToInt32(beatInformation.Split(' ')[1]);
        startingPoint = (float)bpm / divider;

        //1초마다 떨어지는 비트 개수
        beatCount = (float)bpm / divider;

        //비트가 떨어지는 간격 시간 계산
        beatInterval = 1 / beatCount;

        //각 비트들이 떨어지는 위치 및 시간 정보 읽기
        string line;
        while((line = reader.ReadLine()) != null)
        {
            Note note = new Note(
                Convert.ToInt32(line.Split(' ')[0]) + 1,
                Convert.ToInt32(line.Split(' ')[1])
            );
            notes.Add(note);
        }


        //모든 노트를 정해진 시간에 출발하도록 설정
        for(int i = 0; i < notes.Count; i++){
            StartCoroutine(AwaiteMakeNote(notes[i]));
        }
        //마지막 노트를 기준으로 게임종료 함수 불러옴
        StartCoroutine(AwaitGameResult(notes[notes.Count - 1].order));
    }


    IEnumerator AwaitGameResult(int order)
    {
        yield return new WaitForSeconds(startingPoint + order * beatInterval + 8.0f);
        GameResult();
    }

    void GameResult()
    {
        SceneManager.LoadScene("GameResultScene");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
