using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    private bool stateInTransition = false;
    private int currentAction = 0;

    private Vector3 initialPos;
    private Vector3 targetPos;
    private float transitionTime;

    private Stack<int> actions;

    private bool planFollower = true;
    private float transitionPeriod = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        actions = new Stack<int>();
        loadPlan();
    }

    void loadPlan() {
        hardCoded();
    }

    void hardCoded() {
        // reversed, 8 west 1 north, 2 west 2 north, 1 west 2 north 2 east
        Add(9, -2);
        Add(1, 1);
        Add(2, -2);
        Add(2, 1);
        Add(1, -2);
        Add(2, 1);
        Add(2, 2);
    }

    void Add(int amount, int dir) {
        int i = 0;
        while (i < amount) {
            actions.Push(dir);
            i++;
        }
    }

    void FixedUpdate() 
    {
        //Debug.Log(transform.localPosition);
        // AI controller, plan follower
        if (planFollower) {
            if (stateInTransition == false) {
                if (actions.Count > 0) {
                    currentAction = actions.Pop();

                    // begin state transition
                    initialPos = transform.localPosition;
                    stateInTransition = true;
                    SetTarget(currentAction);
                    Move();
                }
            } else {
                if (currentAction != 0) {
                    Move();
                }
            }
        }
    }

    /* + 1 north, -1 south, + 2 east, - 2 west */
    void SetTarget(int dir) {// Need to set targets from recovery transitions as well
        if (dir == 1) {
            targetPos = new Vector3(transform.localPosition.x, transform.localPosition.y,transform.localPosition.z + dir);
        } else if (dir == -1) {
            targetPos = new Vector3(transform.localPosition.x, transform.localPosition.y,transform.localPosition.z + dir);
        } else if (dir == 2) {
            targetPos = new Vector3(transform.localPosition.x + dir/2, transform.localPosition.y,transform.localPosition.z);
        } else if (dir == -2) {
            targetPos = new Vector3(transform.localPosition.x + dir/2, transform.localPosition.y,transform.localPosition.z);
        }
    }

    void Move() {
        transitionTime += Time.deltaTime;
        if (transitionTime >= transitionPeriod) {
            EndTransition(); // Handle cancel of a transition too
            transform.localPosition = targetPos;
        } else {
            transform.localPosition = Vector3.Lerp(initialPos, targetPos, transitionTime*1.0f/transitionPeriod);
        }
    }

    void EndTransition() {
        stateInTransition = false;
        transitionTime = 0;
        currentAction = 0;
    }
    /*
    void ApplyAction(int action) {

    }*/
}
