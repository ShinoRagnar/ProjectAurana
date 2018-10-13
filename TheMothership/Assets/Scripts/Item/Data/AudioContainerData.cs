using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//A list of itemdata
[Serializable]
public struct AudioData
{
    public SoundWhen when;
    public AudioNames audio;
    public float volume;
    public float pitch;
}
[CreateAssetMenu(fileName = "NewAudioContainer", menuName = "Game/AudioContainer", order = 30)]
public class AudioContainerData : ScriptableObject
{
    [SerializeField]
    public string enumName;
    [SerializeField]
    public AudioData[] audioFiles;
}
