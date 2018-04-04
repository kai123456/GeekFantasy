﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

	public static Player _instance;
	public GameObject initialPanel;
	//heyi
	public int maxplayerLife;
	public int curplayerLife = 10;
	public int attackPower = 1;
	public float volume;

	public GameObject gameOver;
	public GameObject finish;
	public bool flag;       //if true then the figure cannot move or attack
	public bool isInitial;
	public GameObject bulletPrefab;
	public float moveSpeed = 9;
	public Vector3 bulletEulerAngles;

	private Animator anim;
	// private SpriteRenderer sr;
	public Sprite[] playerSprite;
	public GameObject[] enemyArray;
	public int num;
	public int Array_size = 0;
	private Vector2 dest = Vector2.zero;
    private float targetTime=0; //use for control the bullets' cooldown time
    private float isimmune; //use for control the figure's immune time


    // up right left down


    private void Awake ()
	{
		anim = GetComponent<Animator> ();
		// sr = GetComponent<SpriteRenderer> ();
		_instance = this;
	}
	// Use this for initialization
	void Start ()
	{	
		finish.SetActive (false);
		initialPanel.SetActive (true);
		maxplayerLife = curplayerLife;
		enemyArray = new GameObject[30];
		dest = transform.position;
		flag = false;
		isInitial = false;
		var objectsn = GameObject.FindObjectsOfType (typeof(GameObject));
		for (int i = 0; i < objectsn.Length; i++) {
			if (objectsn [i].name.Length == 7 && objectsn [i].name [0] == 'E' && objectsn [i].name [1] == 'n' && objectsn [i].name [2] == 'e' && objectsn [i].name [3] == 'm' && objectsn [i].name [4] == 'y') {
				int tmp = (objectsn [i].name [5] - '0') * 10 + (objectsn [i].name [6] - '0');
				enemyArray [tmp] = (GameObject)objectsn [i];
				Array_size++;
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		
		if (Input.GetKeyDown (KeyCode.C)) {
			initialPanel.SetActive (false);
			isInitial = true;
		}
		if (isInitial == true) {
			Attack ();
		}
	}

	public bool hitself (Vector2 dir, int num)
	{
		Vector2 pos = transform.position;
		RaycastHit2D hit = Physics2D.Linecast (dir + 1.5f * (dir - pos), pos);
//		Debug.DrawRay (dir + dir - pos, (pos - dir) * 100, Color.blue);
//		Debug.Log (hit.collider);
		return (hit.collider == GetComponent<CapsuleCollider2D> ()/*&& hit.collider != enemyArray[num].GetComponent<Collider2D>()*/);
	}

	public void FixedUpdate ()
	{
		if (curplayerLife <= 0) {
			Time.timeScale = 0;
			gameOver.SetActive (true);
		}

		if (flag == true) {
			Vector2 temp = Vector2.MoveTowards (transform.position, dest, 0.5f);
            //this.GetComponent<Renderer>().enabled = !this.GetComponent<Renderer>().enabled;
            if (!hitself (temp, num)) {
				//temp = transform.position;
				flag = false;
			}
			GetComponent<Rigidbody2D> ().MovePosition (temp);
			float detect_distance = Vector3.Distance (enemyArray [num].transform.position, transform.position);
			if (detect_distance > 4)
				flag = false;

		}


		if (flag == false && isInitial == true) {
			float h = Input.GetAxisRaw ("Horizontal");
			float v = Input.GetAxisRaw ("Vertical");
			Vector2 movement_vector = new Vector2 (h, v);
			transform.Translate (Vector3.right * h * moveSpeed * Time.deltaTime, Space.World);
			if (movement_vector != Vector2.zero) {
				anim.SetBool ("iswalking", true);
				anim.SetFloat ("input_x", movement_vector.x);
				anim.SetFloat ("input_y", movement_vector.y);
			} else {
				anim.SetBool ("iswalking", false);
			}

			if (h < 0) {
				//sr.sprite = playerSprite [2];
				bulletEulerAngles = new Vector3 (0, 0, 90);

			} else if (h > 0) {
				//sr.sprite = playerSprite [1];
				bulletEulerAngles = new Vector3 (0, 0, -90);
			}

			transform.Translate (Vector3.up * v * moveSpeed * Time.deltaTime, Space.World);

			if (v < 0) {
				//sr.sprite = playerSprite [3];
				bulletEulerAngles = new Vector3 (0, 0, -180);
			} else if (v > 0) {
				//sr.sprite = playerSprite [0];
				bulletEulerAngles = new Vector3 (0, 0, 0);
			}

			for (int i = 0; i < Array_size; i++) {
                //make sure enemyArray is not NULL
				if (!enemyArray [i])
					continue;

                //calculate the distance between figure and enemy
				float distance = Vector3.Distance (enemyArray [i].transform.position, transform.position);
				if (i == 0)
					volume = 3f;
				else
					volume = 2.2f;

				if (distance < volume) {
                    //disable the move and attack
					flag = true;

                    //set immune and the figure will flash
                    if (Time.time>isimmune)
                    {
                        gameObject.SendMessage("Flash", this.gameObject, SendMessageOptions.DontRequireReceiver);
                        curplayerLife = curplayerLife - 1;
                        isimmune = isimmune=Time.time+1.2f;
                    }

                    //record the the # of attcking enemy
					num = i;

                    //calculate the destination of bouncing
					dest = (Vector2)transform.position + volume * ((Vector2)transform.position - (Vector2)enemyArray [num].transform.position);
				}
			}

		}
	}

	private void Attack ()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {

            if (Time.time > targetTime)
            {
                Instantiate(bulletPrefab, transform.position, Quaternion.Euler(transform.eulerAngles + bulletEulerAngles));
                targetTime = Time.time + 0.4f;
            }



		}
	}

	public void Finish ()
	{
		finish.SetActive (true);
	}

    public IEnumerator Flash(GameObject obj)
    {
        obj.GetComponent<Renderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        obj.GetComponent<Renderer>().enabled = true;
        yield return new WaitForSeconds(0.2f);
        obj.GetComponent<Renderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        obj.GetComponent<Renderer>().enabled = true;
        yield return new WaitForSeconds(0.2f);
        obj.GetComponent<Renderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        obj.GetComponent<Renderer>().enabled = true;
        yield return new WaitForSeconds(0.2f);
    }

}