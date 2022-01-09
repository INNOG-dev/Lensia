using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RegistryManager
{

    public static Registry<Item> itemRegistry = new Registry<Item>();

    public static Registry<Emoji> emojiRegistry = new Registry<Emoji>();

    public static Registry<PlayerSkin> skinRegistry = new Registry<PlayerSkin>();

    public static Registry<Map> mapRegistry = new Registry<Map>();

    public static Registry<AudioClip> audioRegistry = new Registry<AudioClip>();

    public static Registry<ContainerType> containerRegistry = new Registry<ContainerType>();


    public static void register()
    {
        registerAudioClips();
        registerEmoji();
        registerItems();
        registerSkins();
        registerMaps();
        registerContainer();
        registerEntities();
    }

    public static void registerAudioClips()
    {
        List<AudioClip> clips = new List<AudioClip>();

        clips.Add(new AudioClip("Running")); //0
        clips.Add(new AudioClip("Lands")); //1
        clips.Add(new AudioClip("Grince")); //2
        clips.Add(new AudioClip("BoostJump")); //3

        audioRegistry.registerAll(clips.ToArray());
    }

    public static void registerEmoji()
    {
        List<Emoji> emoji = new List<Emoji>();

        emoji.Add(new Emoji("Grinning Face",0));


        emojiRegistry.registerAll(emoji.ToArray());
    }

    public static void registerItems()
    {

    }

    public static void registerSkins()
    {
        List<PlayerSkin> skins = new List<PlayerSkin>();

        skins.Add(new PlayerSkin("Hostess"));
        skins.Add(new PlayerSkin("Hostess2"));
        skins.Add(new PlayerSkin("Hostess3"));

        skinRegistry.registerAll(skins.ToArray());
    }

    public static void registerMaps()
    {
        List<Map> maps = new List<Map>();

        maps.Add(new Map("Ile abandonné (Zone de crash du dirigeable)"));
        maps.Add(new Map("Ile abandonné"));
        maps.Add(new Map("Prison"));
        maps.Add(new Map("Ile abandonné"));
        maps.Add(new Map("Ile abandonné"));
        maps.Add(new Map("Ile abandonné"));
        maps.Add(new Map("Ile abandonné"));
        maps.Add(new Map("Ile abandonné"));

        mapRegistry.registerAll(maps.ToArray());
    }

    public static void registerContainer()
    {
        List<ContainerType> containers = new List<ContainerType>();

        containers.Add(new ContainerType(typeof(AvatarEditionContainer),"Avatar edition container"));
        containers.Add(new ContainerType(typeof(SocialContainer), "Social container"));

        containerRegistry.registerAll(containers.ToArray());
    }

    public static void registerEntities()
    {
        EntityRegistry.registerEntity(typeof(Player), new EntityType("player"));
        EntityRegistry.registerEntity(typeof(EntityBoomer), new EntityType("boomer"));
    }

}
