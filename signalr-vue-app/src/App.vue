<template>
  <div id="app">
    <h1 v-if="!isChatting">Enter Group ID and Name</h1>
    
    <!-- Group ID and Name Input Form -->
    <div v-if="!isChatting" class="form-container">
      <input
        v-model="groupId"
        type="text"
        placeholder="Enter Group ID"
        class="input-field"
      />
      <input
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
        <span>Welcome, {{ userName }}</span>
      </div>

      <div class="messages" ref="messagesContainer">
        <div v-for="(msg, index) in messages" :key="index" class="message">
          <strong>{{ msg.user }}:</strong> {{ msg.content }}
        </div>
      </div>

      <div class="message-input">
        <textarea
          v-model="newMessage"
          placeholder="Type a message..."
          rows="3"
          class="input-field"
        ></textarea>
        <button @click="sendMessage" class="submit-btn">Send</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { HubConnectionBuilder } from '@microsoft/signalr';
import { ref, reactive, nextTick } from 'vue';

// Reactive state for component
const isChatting = ref(false);
const groupId = ref('');
const userName = ref('');
const newMessage = ref('');
const messages = reactive([]);

const conn = new HubConnectionBuilder()
  .withUrl(`https://localhost:44347/chatHub`)
  .build();

conn.on('JoinSpecificChatRoom', (user, message) => {
  messages.push({ user, content: message });
});

conn.on('ReceiveSpecificMessage', (user, message) => {
  messages.push({ user, content: message });
});

const startChat = async () => {
  try {
    const userConnection = {
      UserName: userName.value,
      ChatRoom: groupId.value
    };

    await conn.start();

    await conn.invoke('JoinSpecificChatRoom', userConnection);

    isChatting.value = true;
  } catch (error) {
    console.error('Error starting chat:', error);
  }
};

const sendMessage = async () => {
  if (newMessage.value.trim()) {
    try {
      await conn.invoke('SendMessage', newMessage.value);

      newMessage.value = '';

      nextTick(() => {
        const messagesContainer = document.querySelector('.messages');
        if (messagesContainer) {
          messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
      });
    } catch (error) {
      console.error('Error sending message:', error);
    }
  }
};
</script>

<style scoped>
#app {
  font-family: Arial, sans-serif;
  padding: 20px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: #f4f4f4;
}

.form-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 15px;
}

.input-field {
  padding: 10px;
  font-size: 16px;
  width: 250px;
  margin-bottom: 10px;
  border-radius: 5px;
  border: 1px solid #ccc;
}

.submit-btn {
  padding: 10px 15px;
  background-color: #4caf50;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
}

.submit-btn:hover {
  background-color: #45a049;
}

.chat-container {
  background-color: #fff;
  border-radius: 10px;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 600px;
  padding: 20px;
  display: flex;
  flex-direction: column;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 15px;
  font-weight: bold;
}

.messages {
  max-height: 300px;
  overflow-y: auto;
  margin-bottom: 15px;
}

.message {
  background-color: #f1f1f1;
  border-radius: 5px;
  padding: 10px;
  margin-bottom: 5px;
}

.message strong {
  color: #4caf50;
}

.message-input {
  display: flex;
  gap: 10px;
}

textarea {
  padding: 10px;
  font-size: 16px;
  border-radius: 5px;
  width: 100%;
  resize: none;
}
</style>
