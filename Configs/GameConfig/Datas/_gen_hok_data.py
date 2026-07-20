"""生成 hok_skill.json 和 hok_buff.json，数据严格来自 ResSkillConfigs.cs / ResBuffConfigs.cs。"""
import json
from pathlib import Path

BASE = Path("E:/Unity.game/HOK/TEngine/Configs/GameConfig/Datas")

# TargetCfg 构造辅助：None 字段省略以保持简洁
def T(team=None, rule=None, types=None, rng=None, search=None):
    d = {}
    if team is not None: d["targetTeam"] = team
    if rule is not None: d["selectRule"] = rule
    if types is not None: d["targetTypeArr"] = types
    if rng is not None: d["selectRange"] = rng
    if search is not None: d["searchDis"] = search
    return d

def B(btype, name, path, speed, size, height, offset, delay=0, canBlock=None, impacter=None, duration=0):
    d = {"bulletType": btype, "bulletName": name, "resPath": path, "bulletSpeed": speed,
         "bulletSize": size, "bulletHeight": height, "bulletOffset": offset, "bulletDelay": delay, "bulletDuration": duration}
    if canBlock is not None: d["canBlock"] = canBlock
    if impacter is not None: d["impacter"] = impacter
    return d

HST = ["Hero", "Soldier", "Tower"]
HST2 = ["Hero", "Tower", "Soldier"]
HS = ["Hero", "Soldier"]
HT = ["Hero"]

# ============ 技能数据 ============
skills = {
    1010: {"id":1010,"skillID":1010,"aniName":"atk","releaseMode":"None",
           "targetCfg":T("Enemy","TargetClosestSingle",HST,2,10),
           "spellTime":800,"isNormalAttack":True,"skillTime":1400,"damage":45,
           "audioStart":"arthur_ska_rls","audioHit":"arthur_ska_hit"},
    1011: {"id":1011,"skillID":1011,"iconName":"arthur_sk1","releaseMode":"Click",
           "cdTime":5000,"buffIDArr":[10110,10111],"audioStart":"arthur_sk1_rls"},
    1012: {"id":1012,"skillID":1012,"iconName":"arthur_sk2","releaseMode":"Click",
           "cdTime":5000,"buffIDArr":[10120],"audioStart":"arthur_sk2_rls"},
    1013: {"id":1013,"skillID":1013,"iconName":"arthur_sk3","aniName":"sk3","releaseMode":"Click",
           "targetCfg":T("Enemy","TargetClosestSingle",HT,4,10),
           "cdTime":10000,"spellTime":250,"skillTime":1300,"buffIDArr":[10130,10131,10132,10133],
           "audioStart":"arthur_sk3_rls"},
    1014: {"id":1014,"skillID":1010,"aniName":"sk1_atk","releaseMode":"None",
           "targetCfg":T("Enemy","TargetClosestSingle",HST,2,10),
           "spellTime":800,"isNormalAttack":True,"skillTime":1400,"damage":90,
           "buffIDArr":[10140,10141,10142],"audioHit":"arthur_sk1_hit"},
    1020: {"id":1020,"skillID":1020,"aniName":"atk","releaseMode":"None",
           "targetCfg":T("Enemy","TargetClosestSingle",HST2,5,15),
           "bulletCfg":B("SkillTarget","后羿普攻子弹","houyi_ska_bullet",1,0.1,1.5,0.5,0),
           "spellTime":550,"isNormalAttack":True,"skillTime":1400,"damage":50,
           "audioWork":"houyi_ska_rls","audioHit":"com_hit2"},
    1021: {"id":1021,"skillID":1021,"iconName":"houyi_sk1","releaseMode":"Click",
           "cdTime":5000,"buffIDArr":[10210],"audioStart":"houyi_sk1_rls"},
    1022: {"id":1022,"skillID":1022,"iconName":"houyi_sk2","aniName":"sk2","releaseMode":"Postion",
           "targetCfg":T("Dynamic",None,None,6,None),
           "cdTime":5000,"spellTime":630,"skillTime":1200,"buffIDArr":[10220,10221,10222,10223],
           "audioStart":"houyi_sk2_rls"},
    1023: {"id":1023,"skillID":1023,"iconName":"houyi_sk3","aniName":"sk3","releaseMode":"Direction",
           "bulletCfg":B("UIDirection","后羿大招-灼日之矢","houyi_sk3_bullet",1,0.5,1.5,1,0,True,
                         T("Enemy","Hero",HT,None,None),5000),
           "cdTime":8000,"spellTime":230,"skillTime":800,"buffIDArr":[10230,10231],
           "audioStart":"houyi_sk3_rls","audioHit":"houyi_sk3_hit"},
    1024: {"id":1024,"skillID":1024,"aniName":"atk","releaseMode":"None",
           "targetCfg":T("Enemy","TargetClosestSingle",HST,5,15),
           "bulletCfg":B("SkillTarget","后羿1技能强化普攻子弹","houyi_ska_bullet_skenhance",1,0.1,1.5,0.5,0),
           "spellTime":550,"isNormalAttack":True,"skillTime":1400,"damage":40,
           "buffIDArr":[10240],"audioWork":"houyi_ska_rls","audioHit":"com_hit2"},
    1025: {"id":1025,"skillID":1025,"aniName":"atk","releaseMode":"None",
           "targetCfg":T("Enemy","TargetClosestSingle",HST,5,15),
           "bulletCfg":B("SkillTarget","后羿被动强化普攻子弹","houyi_ska_bullet_edenhance",1,0.1,1.5,0.5,0),
           "spellTime":550,"isNormalAttack":True,"skillTime":1400,"damage":20,
           "buffIDArr":[10250],"audioWork":"houyi_ska_multiarrow","audioHit":"houyi_multi_hit"},
    1026: {"id":1026,"skillID":1024,"aniName":"atk","releaseMode":"None",
           "targetCfg":T("Enemy","TargetClosestSingle",HST,5,15),
           "bulletCfg":B("SkillTarget","后羿技能强化普攻子弹","houyi_ska_bullet_edskmixed",1,0.1,1.5,0.5,0),
           "spellTime":550,"isNormalAttack":True,"skillTime":1400,"damage":100,
           "buffIDArr":[10260],"audioWork":"houyi_ska_multiarrow","audioHit":"houyi_multi_hit"},
    10010: {"id":10010,"skillID":10010,"releaseMode":"None",
            "targetCfg":T("Enemy","TargetClosestSingle",HS,6,0),
            "bulletCfg":B("SkillTarget","蓝方防御塔攻击子弹","tower_ska_bullet",1,0.1,4,0,0),
            "spellTime":1000,"isNormalAttack":True,"skillTime":2000,"damage":50,
            "audioWork":"tower_ska_rls","audioHit":"tower_ska_hit"},
    10020: {"id":10020,"skillID":10020,"releaseMode":"None",
            "targetCfg":T("Enemy","TargetClosestSingle",HS,6,0),
            "bulletCfg":B("SkillTarget","蓝方水晶攻击子弹","tower_ska_bullet",1,0.1,2.5,0,0),
            "spellTime":1000,"isNormalAttack":True,"skillTime":2000,"damage":100,
            "audioWork":"tower_ska_rls","audioHit":"tower_ska_hit"},
    10030: {"id":10030,"skillID":10030,"aniName":"attack","releaseMode":"None",
            "targetCfg":T("Enemy","TargetClosestSingle",HST,1.5,5),
            "spellTime":400,"isNormalAttack":True,"skillTime":1200,"damage":20},
    10040: {"id":10040,"skillID":10040,"aniName":"attack","releaseMode":"None",
            "targetCfg":T("Enemy","TargetClosestSingle",HST,4,7),
            "bulletCfg":B("SkillTarget","蓝方防远程小兵攻击子弹","bluesoldier_ska_bullet",0.5,0.1,0.6,0,0),
            "spellTime":400,"isNormalAttack":True,"skillTime":1200,"damage":30},
    20010: {"id":20010,"skillID":20010,"releaseMode":"None",
            "targetCfg":T("Enemy","TargetClosestSingle",HS,6,0),
            "bulletCfg":B("SkillTarget","红方防御塔攻击子弹","tower_ska_bullet",1,0.1,4,0,0),
            "spellTime":1000,"isNormalAttack":True,"skillTime":2000,"damage":50,
            "audioWork":"tower_ska_rls","audioHit":"tower_ska_hit"},
    20020: {"id":20020,"skillID":20020,"releaseMode":"None",
            "targetCfg":T("Enemy","TargetClosestSingle",HS,6,0),
            "bulletCfg":B("SkillTarget","红方水晶攻击子弹","tower_ska_bullet",1,0.1,2.5,0,0),
            "spellTime":1000,"isNormalAttack":True,"skillTime":2000,"damage":100,
            "audioWork":"tower_ska_rls","audioHit":"tower_ska_hit"},
    20030: {"id":20030,"skillID":20030,"aniName":"attack","releaseMode":"None",
            "targetCfg":T("Enemy","TargetClosestSingle",HST,1.5,5),
            "spellTime":400,"isNormalAttack":True,"skillTime":1200,"damage":20},
    20040: {"id":20040,"skillID":20040,"aniName":"attack","releaseMode":"None",
            "targetCfg":T("Enemy","TargetClosestSingle",HST,4,7),
            "bulletCfg":B("SkillTarget","红方防远程小兵攻击子弹","redsoldier_ska_bullet",0.5,0.1,0.6,0,0),
            "spellTime":400,"isNormalAttack":True,"skillTime":1200,"damage":30},
}

# ============ Buff 数据（多态 $type 分发）============
# 基类通用字段：attacher, impacter, buffDelay, buffInterval, buffDuration, staticPosType, buffAudio, buffEffect, hitTickAudio
def base(buffID, name, btype, attacher, delay=0, interval=0, duration=0, static="None", audio=None, effect=None, hitAudio=None, impacter=None):
    d = {"buffID":buffID, "buffName":name, "buffType":btype, "attacher":attacher,
         "buffDelay":delay, "buffInterval":interval, "buffDuration":duration, "staticPosType":static}
    if audio: d["buffAudio"]=audio
    if effect: d["buffEffect"]=effect
    if hitAudio: d["hitTickAudio"]=hitAudio
    if impacter: d["impacter"]=impacter
    return d

buffs = {
    90000: {"$type":"BuffCfg", "id":90000, **base(90000,"移动攻击","MoveAttack","Caster",interval=66,duration=5000)},
    10100: {"$type":"HPCureBuffCfg", "id":10100, **base(10100,"被动治疗","HPCure","Caster",interval=2000,duration=-1), "cureHPpct":2},
    10110: {"$type":"MoveSpeedBuffCfg", "id":10110, **base(10110,"加速","MoveSpeed_Single","Caster",duration=3000), "amount":30},
    10111: {"$type":"CommonModifySkillBuffCfg", "id":10111, **base(10111,"替换普攻","ModifySkill","Caster",duration=3000), "originalID":1010, "replaceID":1014},
    10140: {"$type":"BuffCfg", "id":10140, **base(10140,"沉默","Silense","Target",duration=1000)},
    10141: {"$type":"ArthurMarkBuffCfg", "id":10141, **base(10141,"Arthur1技能标记","ArthurMark","Target",duration=5000), "damagePct":1},
    10142: {"$type":"MoveSpeedBuffCfg", "id":10142, **base(10142,"范围友军加速","MoveSpeed_DynamicGroup","Target",
              impacter=T("Enemy","TargetClosetMultiple",HT,5),interval=66,duration=5000), "amount":10},
    10120: {"$type":"DamageDynamicGroupBuffCfg", "id":10120, **base(10120,"范围伤害","Damage_DynamicGroup","Caster",
              impacter=T("Enemy","TargetClosetMultiple",["Hero","Soldier"],2),interval=1000,duration=5000,hitAudio="com_hit1",effect="Effect_sk2"), "damage":100},
    10130: {"$type":"TargetFlashMoveBuffCfg", "id":10130, **base(10130,"目标闪现跳跃","TargetFlashMove","Caster"), "offset":1.5},
    10131: {"$type":"ExecuteDamageBuffCfg", "id":10131, **base(10131,"百分比生命伤害","ExecuteDamage","Target"), "damagePct":12},
    10132: {"$type":"BuffCfg", "id":10132, **base(10132,"范围击飞","Knockup_Group","Target",
              impacter=T("Friend","TargetClosetMultiple",["Hero","Soldier"],2),delay=100,duration=500)},
    10133: {"$type":"DamageStaticGroupBuffCfg", "id":10133, **base(10133,"固定位置范围伤害","Damage_StaticGroup","Indie",
              impacter=T("Enemy","PositionClosestMultiple",["Hero","Soldier"],2),delay=100,interval=1000,duration=5000,static="SkillLockTargetPos",effect="Effect_sk3",hitAudio="com_hit1"), "damage":50},
    10200: {"$type":"HouyiPasvAttackSpeedBuffCfg", "id":10200, **base(10200,"被动攻速加成叠加","HouyiPasvAttackSpeed","Caster",interval=66,duration=-1),
            "overCount":3, "speedAddtion":10, "resetTime":3000},
    10201: {"$type":"HouyiMultipleSkillModifyBuffCfg", "id":10201, **base(10201,"被动普攻修改buff","HouyiPasvSkillModify","Caster",interval=66,duration=-1),
            "originalID":1020, "powerID":1025, "superPowerID":1026, "triggerOverCount":3, "resetTime":3000},
    10250: {"$type":"HouyiMultipleArrowBuffCfg", "id":10250, **base(10250,"1技能强化普攻为多重射击","HouyiPasvMultiArrow","Caster"),
            "arrowCount":2, "arrowDelay":100, "posOffset":0.3},
    10210: {"$type":"HouyiScatterSkillModifyBuffCfg", "id":10210, **base(10210,"技能强化普攻","HouyiActiveSkillModify","Caster",duration=5000),
            "originalID":1020, "powerID":1024, "superPowerID":1026},
    10240: {"$type":"HouyiScatterArrowBuffCfg", "id":10240, **base(10240,"1技能强化普攻为散射","Scatter","Caster"),
            "targetCfg":T("Enemy","TargetClosetMultiple",["Hero","Soldier"],5), "scatterCount":2, "damagePct":50},
    10260: {"$type":"HouyiMixedMultiScatterBuffCfg", "id":10260, **base(10260,"1技能强化普攻为散射","HouyiMixedMultiScatter","Caster"),
            "targetCfg":T("Enemy","TargetClosetMultiple",["Hero","Soldier"],5),
            "scatterCount":2, "damagePct":50, "arrowCount":2, "arrowDelay":50, "posOffset":0.3},
    10220: {"$type":"DamageStaticGroupBuffCfg", "id":10220, **base(10220,"后羿2技能范围伤害1","Damage_StaticGroup","Indie",
              impacter=T("Enemy","PositionClosestMultiple",["Hero","Soldier"],2),duration=2000,static="UIInputPos",effect="houyi_sk2_effect"), "damage":100},
    10221: {"$type":"DamageStaticGroupBuffCfg", "id":10221, **base(10221,"后羿2技能额外范围伤害2","Damage_StaticGroup","Indie",
              impacter=T("Enemy","PositionClosestMultiple",["Hero","Soldier"],1),static="UIInputPos"), "damage":50},
    10222: {"$type":"MoveSpeedBuffCfg", "id":10222, **base(10222,"后羿2技能动态范围移速1","MoveSpeed_StaticGroup","Indie",
              impacter=T("Enemy","PositionClosestMultiple",["Hero","Soldier"],2),duration=2000,static="UIInputPos"), "amount":-30},
    10223: {"$type":"MoveSpeedBuffCfg", "id":10223, **base(10223,"后羿2技能额外动态范围移速2","MoveSpeed_StaticGroup","Indie",
              impacter=T("Enemy","PositionClosestMultiple",["Hero","Soldier"],1),duration=2000,static="UIInputPos"), "amount":-20},
    10230: {"$type":"StunDynamicTimeBuffCfg", "id":10230, **base(10230,"动态时间眩晕","Stun_Single_DynamicTime","Bullet"),
            "minStunTime":1000, "maxStunTime":3500},
    10231: {"$type":"DamageStaticGroupBuffCfg", "id":10231, **base(10231,"范围伤害","Damage_StaticGroup","Bullet",
              impacter=T("Friend","PositionClosestMultiple",["Hero","Soldier"],2),static="BulletHitTargetPos"), "damage":100},
}

# 写 JSON（Luban 使用 *@file.json 作为 input 时，会按数组读取多条记录）
def normalize_target(target):
    if target is None:
        return None
    return {
        "targetTeam": target.get("targetTeam", "Dynamic"),
        "selectRule": target.get("selectRule", "None"),
        "targetTypeArr": target.get("targetTypeArr", []),
        "selectRange": target.get("selectRange", 0),
        "searchDis": target.get("searchDis", 0),
    }


def normalize_bullet(bullet):
    if bullet is None:
        return None
    return {
        "bulletType": bullet.get("bulletType", "SkillTarget"),
        "bulletName": bullet.get("bulletName", ""),
        "resPath": bullet.get("resPath", ""),
        "bulletSpeed": bullet.get("bulletSpeed", 0),
        "bulletSize": bullet.get("bulletSize", 0),
        "bulletHeight": bullet.get("bulletHeight", 0),
        "bulletOffset": bullet.get("bulletOffset", 0),
        "bulletDelay": bullet.get("bulletDelay", 0),
        "canBlock": bullet.get("canBlock", False),
        "impacter": normalize_target(bullet.get("impacter")),
        "bulletDuration": bullet.get("bulletDuration", 0),
    }


def normalize_skill(skill):
    return {
        "id": skill.get("id", skill.get("skillID", 0)),
        "skillID": skill.get("skillID", 0),
        "iconName": skill.get("iconName", ""),
        "aniName": skill.get("aniName", ""),
        "releaseMode": skill.get("releaseMode", "None"),
        "targetCfg": normalize_target(skill.get("targetCfg")),
        "bulletCfg": normalize_bullet(skill.get("bulletCfg")),
        "cdTime": skill.get("cdTime", 0),
        "spellTime": skill.get("spellTime", 0),
        "isNormalAttack": skill.get("isNormalAttack", False),
        "skillTime": skill.get("skillTime", 0),
        "damage": skill.get("damage", 0),
        "buffIDArr": skill.get("buffIDArr", []),
        "audioStart": skill.get("audioStart", ""),
        "audioWork": skill.get("audioWork", ""),
        "audioHit": skill.get("audioHit", ""),
    }


BUFF_SUBCLASS_DEFAULTS = {
    "HPCureBuffCfg": {"cureHPpct": 0},
    "MoveSpeedBuffCfg": {"amount": 0},
    "CommonModifySkillBuffCfg": {"originalID": 0, "replaceID": 0},
    "ArthurMarkBuffCfg": {"damagePct": 0},
    "TargetFlashMoveBuffCfg": {"offset": 0},
    "ExecuteDamageBuffCfg": {"damagePct": 0},
    "DamageDynamicGroupBuffCfg": {"damage": 0},
    "DamageStaticGroupBuffCfg": {"damage": 0},
    "StunDynamicTimeBuffCfg": {"minStunTime": 0, "maxStunTime": 0},
    "HouyiPasvAttackSpeedBuffCfg": {"overCount": 0, "speedAddtion": 0, "resetTime": 0},
    "HouyiMultipleSkillModifyBuffCfg": {"originalID": 0, "powerID": 0, "superPowerID": 0, "triggerOverCount": 0, "resetTime": 0},
    "HouyiScatterSkillModifyBuffCfg": {"originalID": 0, "powerID": 0, "superPowerID": 0},
    "HouyiScatterArrowBuffCfg": {"scatterCount": 0, "targetCfg": None, "damagePct": 0},
    "HouyiMultipleArrowBuffCfg": {"arrowCount": 0, "arrowDelay": 0, "posOffset": 0},
    "HouyiMixedMultiScatterBuffCfg": {"scatterCount": 0, "targetCfg": None, "damagePct": 0, "arrowCount": 0, "arrowDelay": 0, "posOffset": 0},
}


def normalize_buff(buff):
    buff_type_name = buff.get("$type", "BuffCfg")
    result = {
        "$type": buff_type_name if buff_type_name != "BuffCfg" else "CommonBuffCfg",
        "id": buff.get("id", buff.get("buffID", 0)),
        "buffID": buff.get("buffID", 0),
        "buffName": buff.get("buffName", ""),
        "buffType": buff.get("buffType", "None"),
        "attacher": buff.get("attacher", "None"),
        "impacter": normalize_target(buff.get("impacter")),
        "buffDelay": buff.get("buffDelay", 0),
        "buffInterval": buff.get("buffInterval", 0),
        "buffDuration": buff.get("buffDuration", 0),
        "staticPosType": buff.get("staticPosType", "None"),
        "buffAudio": buff.get("buffAudio", ""),
        "buffEffect": buff.get("buffEffect", ""),
        "hitTickAudio": buff.get("hitTickAudio", ""),
    }
    for key, value in BUFF_SUBCLASS_DEFAULTS.get(buff_type_name, {}).items():
        if key.endswith("targetCfg") or key == "targetCfg":
            result[key] = normalize_target(buff.get(key))
        else:
            result[key] = buff.get(key, value)
    return result


(BASE / "hok_skill.json").write_text(json.dumps([normalize_skill(s) for s in skills.values()], ensure_ascii=False, indent=2), encoding="utf-8")
(BASE / "hok_buff.json").write_text(json.dumps([normalize_buff(b) for b in buffs.values()], ensure_ascii=False, indent=2), encoding="utf-8")
print(f"generated: {len(skills)} skills, {len(buffs)} buffs")
