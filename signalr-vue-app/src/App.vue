<template>
  <div id="app">
    <!-- Notification Banner -->
    <div v-if="notification" class="notification-banner">
      {{ notification }}
    </div>

    <div class="chat-card">
      <h1 v-if="!isChatting" class="chat-title">Join a Chat Room</h1>

      <!-- Group ID and Name Input Form -->
      <div v-if="!isChatting" class="form-container">
        <input
          v-model="groupId"
          type="text"
          placeholder="Enter Group ID"
          class="input-field"
        />
        <input
          @keyup.enter="startChat"
          v-model="userName"
          type="text"
          placeholder="Enter Your Name"
          class="input-field"
        />
        <button @click="startChat" class="submit-btn">Start Chat</button>
      </div>

      <!-- Chatbox -->
      <div v-if="isChatting" class="chat-container">
        <div class="chat-header">
          <h2>Group: {{ groupId }}</h2>
          <span class="user-greeting">Welcome, {{ userName }}</span>
        </div>

        <div class="messages" ref="messagesContainer">
          <div
            v-for="(msg, index) in messages"
            :key="index"
            :class="['message', msg.user === userName ? 'me' : 'other']"
          >
            <div class="bubble">
              <strong>{{ msg.user }}:</strong> {{ msg.content }}
            </div>
          </div>
        </div>

        <div style="display: flex; align-items: center">
          <div class="message-input">
            <textarea
              v-model="newMessage"
              @keyup.enter.exact.prevent="sendMessage"
              placeholder="Type a message..."
              rows="2"
              class="input-field"
            ></textarea>
          </div>
          <button @click="sendMessage" class="submit-btn">Send</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { HubConnectionBuilder } from "@microsoft/signalr";
import { ref, reactive, nextTick, onMounted } from "vue";

const isChatting = ref(false);
const groupId = ref("");
const userName = ref("");
const newMessage = ref("");
const messages = reactive([]);
const notification = ref("");

// 👉 Connect to both hubs
const chatConn = new HubConnectionBuilder()
  .withUrl("http://localhost:44347/chatHub")
  .build();

const notifConn = new HubConnectionBuilder()
  .withUrl("http://localhost:44347/notificationHub")
  .build();

// ChatHub listeners
chatConn.on("JoinSpecificChatRoom", (user, message) => {
  messages.push({ user, content: message });
  showNotification(`${user} joined the chat`);
});

chatConn.on("ReceiveSpecificMessage", (user, message) => {
  messages.push({ user, content: message });
  showNotification(`New message from ${user}`);
});

// NotificationHub listeners
notifConn.on("ReceiveNotification", (user, message) => {
  showNotification(`🔔 ${user}: ${message}`);
  showBrowserNotification("New message", `${user}: ${message}`);
});

const showNotification = (msg) => {
  notification.value = msg;
  setTimeout(() => (notification.value = ""), 3000);
};

const startChat = async () => {
  try {
    const userConnection = {
      UserName: userName.value,
      ChatRoom: groupId.value,
    };

    await chatConn.start();
    await notifConn.start();

    await chatConn.invoke("JoinSpecificChatRoom", userConnection);
    await notifConn.invoke("JoinGroup", userConnection); // 👈 join same group in notif hub

    isChatting.value = true;
  } catch (error) {
    console.error("Error starting chat:", error);
  }
};

const sendMessage = async () => {
  if (newMessage.value.trim()) {
    try {
      await chatConn.invoke("SendMessage", newMessage.value);
      await notifConn.invoke("SendNotification", "sent a new message"); // 🔔 optional

      newMessage.value = "";

      nextTick(() => {
        const messagesContainer = document.querySelector(".messages");
        if (messagesContainer) {
          messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
      });
    } catch (error) {
      console.error("Error sending message:", error);
    }
  }
};

const showBrowserNotification = (title, body) => {
  if (Notification.permission === "granted") {
    new Notification(title, {
      body,
      icon: "https://cdn-icons-png.flaticon.com/512/5968/5968322.png", // optional
    });
  }
};

onMounted(() => {
  if ("Notification" in window && Notification.permission !== "granted") {
    Notification.requestPermission();
  }
});
</script>

<style scoped>
* {
  box-sizing: border-box;
}

body,
html {
  margin: 0;
  padding: 0;
  font-family: "Segoe UI", sans-serif;
  background: #e9eff1;
}

#app {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
}

.chat-card {
  background-color: #fff;
  width: 100%;
  max-width: 600px;
  border-radius: 15px;
  padding: 30px;
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);
}

.chat-title {
  margin-bottom: 20px;
  text-align: center;
  color: #333;
}

.form-container {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.input-field {
  padding: 12px;
  font-size: 16px;
  border-radius: 8px;
  border: 1px solid #ccc;
  transition: border-color 0.3s ease;
}

.input-field:focus {
  border-color: #4caf50;
  outline: none;
}

.submit-btn {
  height: fit-content;
  align-items: center;
  height: 100%;
  padding: 12px;
  background-color: #4caf50;
  color: white;
  font-weight: bold;
  font-size: 16px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.submit-btn:hover {
  background-color: #43a047;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  font-weight: bold;
  border-bottom: 1px solid #ccc;
  padding-bottom: 10px;
}

.user-greeting {
  font-size: 14px;
  color: #777;
}

.messages {
  max-height: 300px;
  overflow-y: auto;
  margin-bottom: 15px;
  padding-right: 5px;
  scroll-behavior: smooth;
}

.message {
  display: flex;
  margin-bottom: 10px;
}

.message.me {
  justify-content: flex-end;
}

.message.other {
  justify-content: flex-start;
}

.bubble {
  padding: 10px 15px;
  background-color: #f1f1f1;
  border-radius: 20px;
  max-width: 75%;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
}

.message.me .bubble {
  background-color: #dcf8c6;
  color: #333;
}

.message-input {
  display: flex;
  gap: 10px;
  margin-right: 10px;
}

.notification-banner {
  position: fixed;
  top: 20px;
  background-color: #4caf50;
  color: white;
  padding: 12px 20px;
  border-radius: 10px;
  z-index: 999;
  animation: fade-in-out 3s ease-in-out forwards;
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.2);
}

@keyframes fade-in-out {
  0% {
    opacity: 0;
    transform: translateY(-10px);
  }

  10% {
    opacity: 1;
    transform: translateY(0);
  }

  90% {
    opacity: 1;
    transform: translateY(0);
  }

  100% {
    opacity: 0;
    transform: translateY(-10px);
  }
}
</style>
