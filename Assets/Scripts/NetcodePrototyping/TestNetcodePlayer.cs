using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TestNetcodePlayer : NetworkBehaviour
{
    [SerializeField] private Transform networkObjectPrefab;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData
        {
            _int = 56,
            _bool = true,
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes _fixedString;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref _fixedString);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + "'s number changed from " + previousValue._int + " " + previousValue._bool
                + " to " + newValue._int + " " + newValue._bool + newValue._fixedString);
        };
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            Transform networkObjectTransform = Instantiate(networkObjectPrefab);
            networkObjectTransform.GetComponent<NetworkObject>().Spawn(true);

            //TestServerRpc();
            //randomNumber.Value = new MyCustomData
            //{
            //    _int = Random.Range(1, 100),
            //    _bool = !randomNumber.Value._bool,
            //    _fixedString = "Wot in Tarnation!"
            //};
        }
            
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector3.back;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left;
        }

        float moveSpeed = 3f;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void TestServerRpc()
    {
        //Debug.Log("Connected to server: " + OwnerClientId);
    }
}
