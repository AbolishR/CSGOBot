using SteamKit2;
using System.Collections.Generic;
using SteamTrade;
using System;
using System.Timers;
 
namespace SteamBot
{
    public class KeyUserHandler : UserHandler
    {
        static string BotVersion = "2.5.2";
        static int SellPricePerKey = 31; // THE ITEM U RECEIVE
        static int BuyPricePerKey = 29; // THE ITEM U GIVE
        static int InviteTimerInterval = 2000;
 
		int UserSkinAdded, UserItem1Added, UserItem2Added, UserItem3Added, UserItem4Added, UserItem5Added, UserKeyAdded, BotItem1Added, BotItem2Added, BotItemAdded3, Bot Item4Added, BotItem5Added, BotKeyAdded, InventoryKeys, InvalidItem = 0;
		
 
        bool InGroupChat, TimerEnabled, HasRun, HasErrorRun, ChooseDonate, AskOverpay, IsOverpaying, HasCounted = false;
        bool TimerDisabled = true;
 
        ulong uid;
        SteamID currentSID;
 
        Timer inviteMsgTimer = new System.Timers.Timer(InviteTimerInterval);
 
        public KeyUserHandler(Bot bot, SteamID sid)
            : base(bot, sid)
        {
        }
 
        public override bool OnFriendAdd()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") added me!");
            // Using a timer here because the message will fail to send if you do it too quickly
            inviteMsgTimer.Interval = InviteTimerInterval;
            inviteMsgTimer.Elapsed += (sender, e) => OnInviteTimerElapsed(sender, e, EChatEntryType.ChatMsg);
            inviteMsgTimer.Enabled = true;
            return true;
        }
 
        public override void OnFriendRemove()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") removed me!");
        }
 
        public override void OnMessage(string message, EChatEntryType type)
        {
            message = message.ToLower();
 
            //REGULAR chat commands
            if (message.Contains("buying") || message.Contains("what") || message.Contains("how many") || message.Contains("how much") || message.Contains("price") || message.Contains("selling"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "I trade keys for " + String.Format("{0:0.00}", (BuyPricePerKey / 9.0)) + " , and trade keys for " + String.Format("{0:0.00}", (SellPricePerKey / 9.0)) + ".");
            }
            else if (message.Contains("thank"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "You're welcome!");
            }
            else if (message == "donate")
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Please type that command into the TRADE WINDOW, not here! And thanks. <3");
            }
            // ADMIN commands
            else if (IsAdmin)
            {
                if (message.StartsWith(".join"))
                {
                    // Usage: .join GroupID - e.g. ".join 103582791433582049" or ".join cts" - this will allow the bot to join a group's chatroom
                    if (message.Length >= 7)
                    {
                        if (message.Substring(6) == "tf2")
                        {
                            uid = 103582791430075519;
                        }
                        else
                        {
                            ulong.TryParse(message.Substring(6), out uid);
                        }
                        var chatid = new SteamID(uid);
                        Bot.SteamFriends.JoinChat(chatid);
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Joining chat: " + chatid.ConvertToUInt64().ToString());
                        InGroupChat = true;
                        Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                        Bot.log.Success("Joining chat: " + chatid.ConvertToUInt64().ToString());
                    }
                }
                else if (message.StartsWith(".leave"))
                {
                    // Usage: .leave GroupID, same concept as joining
                    if (message.Length >= 8)
                    {
                        if (message.Substring(7) == "tf2")
                        {
                            uid = 103582791430075519;
                        }
                        else
                        {
                            ulong.TryParse(message.Substring(7), out uid);
                        }
                        var chatid = new SteamID(uid);
                        Bot.SteamFriends.LeaveChat(chatid);
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Leaving chat: " + chatid.ConvertToUInt64().ToString());
                        InGroupChat = false;
                        Bot.log.Success("Leaving chat: " + chatid.ConvertToUInt64().ToString());
                    }
                }
                else if (message.StartsWith(".sell"))
                {
                    // Usage: .sell newprice "e.g. sell 26"
                    int NewSellPrice = 0;
                    if (message.Length >= 6)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Currently selling keys for: " + SellPricePerKey + ".");
                        int.TryParse(message.Substring(5), out NewSellPrice);
                        Bot.log.Success("Admin has requested that I set the new selling price from " + SellPricePerKey + " scrap to " + NewSellPrice + " scrap.");
                        SellPricePerKey = NewSellPrice;
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Setting new selling price to: " + SellPricePerKey + " scrap.");
                        Bot.log.Success("Successfully set new price.");
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "I need more arguments. Current selling price: " + SellPricePerKey + " scrap.");
                    }
                }
                else if (message.StartsWith(".buy"))
                {
                    // Usage: .buy newprice "e.g. .buy 24"
                    int NewBuyPrice = 0;
                    if (message.Length >= 5)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Currently buying keys for: " + BuyPricePerKey + ".");
                        int.TryParse(message.Substring(4), out NewBuyPrice);
                        Bot.log.Success("Admin has requested that I set the new selling price from " + BuyPricePerKey + " scrap to " + NewBuyPrice + " scrap.");
                        BuyPricePerKey = NewBuyPrice;
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Setting new buying price to: " + BuyPricePerKey + " scrap.");
                        Bot.log.Success("Successfully set new price.");
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "I need more arguments. Current buying price: " + BuyPricePerKey + " scrap.");
                    }
                }
                else if (message.StartsWith(".gmessage"))
                {
                    // usage: say ".gmessage Hello!" to the bot will send "Hello!" into group chat
                    if (message.Length >= 10)
                    {
                        if (InGroupChat)
                        {
                            var chatid = new SteamID(uid);
                            string gmessage = message.Substring(10);
                            Bot.SteamFriends.SendChatRoomMessage(chatid, type, gmessage);
                            Bot.log.Success("Said into group chat: " + gmessage);
                        }
                        else
                        {
                            Bot.log.Warn("Cannot send message because I am not in a group chatroom!");
                        }
                    }
                }
                else if (message == ".canceltrade")
                {
                    // Cancels the trade. Occasionally the message will be sent to YOU instead of the current user. Oops.
                    Trade.CancelTrade();
                    Bot.SteamFriends.SendChatMessage(currentSID, EChatEntryType.ChatMsg, "My creator has forcefully cancelled the trade. Whatever you were doing, he probably wants you to stop.");
                }
            }
            else
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.ChatResponse);
            }
        }
 
        public override bool OnTradeRequest()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") has requested to trade with me!");
            return true;
        }
 
        public override void OnTradeError(string error)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID,
                                              EChatEntryType.ChatMsg,
                                              "Error: " + error + "."
                                              );
            Bot.log.Warn(error);
            if (!HasErrorRun)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Did something go horribly wrong? If you have found a bug or something that you think wasn't supposed to happen, please leave a message on my owner's profile!");
                HasErrorRun = true;
            }
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
        }
 
        public override void OnTradeTimeout()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                              "Sorry, but you were either AFK or took too long and the trade was canceled.");
            Bot.log.Info("User was kicked because he was AFK.");
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
        }
 
        public override void OnTradeInit()
        {
            ReInit();
            TradeCountInventory(true);
            Trade.SendMessage("Welcome to AbolishR's CSGO trading bot (v" + BotVersion + "). This bot was coded by http://steamcommunity.com/id/coronaextrabeer. To use this bot, just add your skins or keys, and the bot will automatically add keys or skins when you have eligible skins.");
            if (InventoryKeys == 0)
            {
                Trade.SendMessage("I don't have any keys to sell right now! I am currently buying keys for " + String.Format("{0:0.00}", (BuyPricePerKey / 9.0)) + " ref.");
            }
            else if (InventoryMetal < BuyPricePerKey)
            {
                Trade.SendMessage("I don't have enough skins to buy keys! I am trading keys for " + String.Format("{0:0.00}", (SellPricePerKey / 9.0)) + ".");
            }
            else
            {
                Trade.SendMessage("I am currently trading keys for " + String.Format("{0:0.00}", (BuyPricePerKey / 9.0)) + " , and trading keys for " + String.Format("{0:0.00}", (SellPricePerKey / 9.0)) + " .");
            }
            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);
        }
 
        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            var item = Trade.CurrentSchema.GetItem(schemaItem.Defindex);
            if (!HasCounted)
            {
                Trade.SendMessage("ERROR: I haven't finished counting my inventory yet! Please remove any items you added, and then re-add them or there could be errors.");
            }
            else if (InvalidItem >= 4)
            {
                Trade.CancelTrade();
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Please stop messing around. I am used for buying and selling keys only. I can only accept metal or keys as payment.");
                Bot.log.Warn("Booted user for messing around.");
                Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
            }
            else if (item.Defindex == CHANGE THIS)
            {
                // SKIN 1
                UserSkinAdded++;
                UserItem1Added++;
                Bot.log.Success("User added: " + item.ItemName);
            }
            else if (item.Defindex == CHANGE THIS)
            {
                // SKIN 2
                UserSkinAdded += 3;
                UserItem2Added++;
                Bot.log.Success("User added: " + item.ItemName);
            }
            else if (item.Defindex == CHANGE THIS)
            {
                // SKIN 3
                UserSkinAdded += 9;
                UserItem3Added++;
                Bot.log.Success("User added: " + item.ItemName);
            }
			 else if (item.Defindex == CHANGE THIS)
            {
                // SKIN 4
                UserSkinAdded++;
                UserItem1Added++;
                Bot.log.Success("User added: " + item.ItemName);
            }
			else if (schemaItem.ItemName == "Falchion Case Key")
            {
