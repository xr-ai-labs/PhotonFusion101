using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;

    private Vector3 _forward;
    [Networked] TickTimer delay { get;set; }

    [Networked(OnChanged = nameof(OnBallSpawned)) ] public NetworkBool spawned { get; set; }

    public static void OnBallSpawned(Changed<Player> changed)
    {
        changed.Behaviour.material.color = Color.white;
    }

    private Material _material;

    private Material material
    {
        get
        {
            if(_material==null)
            {
                _material = GetComponentInChildren<MeshRenderer>().material;
            }
            return _material; 
        }
    }

    public override void Render()
    {
        material.color = Color.Lerp(material.color, Color.blue, Time.deltaTime);
    }

    private NetworkCharacterControllerPrototype _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public void Update()
    {
        if(Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey there!");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if(data.direction.sqrMagnitude>0)
            {
                _forward = data.direction;
            }

            if(delay.ExpiredOrNotRunning(Runner))
            {
                if((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall,
                        transform.position+_forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority, (runner, o) =>{
                            o.GetComponent<Ball>().Init();
                        });
                    spawned = !spawned;
                }
                else if((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                        transform.position+_forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority, (runner, o) =>{
                            o.GetComponent<PhysxBall>().Init(_forward * 10);
                        });
                    spawned = !spawned;
                }
            }
        }
    }

    private Text _messages;

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        if (_messages == null)
            _messages = FindObjectOfType<Text>(); 

        if(info.IsInvokeLocal)
        {
            message = $"You said {message}\n";
        }
        else
        {
            message = $"Some other player said {message}\n";
        }

        _messages.text += message;
    }

}
