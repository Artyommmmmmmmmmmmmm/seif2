import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
from collections import Counter

# 1. Загружаем небольшой текст для обучения
text = """Once upon a time there was a little cat.
The cat was very curious about the world.
Every day the cat would explore the garden.
The cat loved to watch the birds and chase butterflies.
One day the cat discovered a magical door in the garden.
The cat was surprised and happy about the discovery."""

# 2. Создаём словарь символов
chars = sorted(list(set(text)))
vocab_size = len(chars)
char_to_idx = {ch: i for i, ch in enumerate(chars)}
idx_to_char = {i: ch for i, ch in enumerate(chars)}

print(f"Уникальных символов: {vocab_size}")
print(f"Символы: {''.join(chars)}")

# 3. Преобразуем текст в числа
data = [char_to_idx[ch] for ch in text]
print(f"Текст в числах: {data[:20]}...")

class MiniLLM(nn.Module):
    """Наша первая языковая модель!"""
    def __init__(self, vocab_size, embedding_dim=64, hidden_dim=128, num_layers=2):
        super().__init__()
        
        # Embedding слой - превращает числа в векторы смыслов
        self.embedding = nn.Embedding(vocab_size, embedding_dim)
        
        # LSTM - "память" модели, запоминает контекст
        self.lstm = nn.LSTM(embedding_dim, hidden_dim, 
                           num_layers, batch_first=True)
        
        # Выходной слой - предсказывает следующий символ
        self.fc = nn.Linear(hidden_dim, vocab_size)
        
    def forward(self, x, hidden=None):
        # Шаг 1: Превращаем числа в векторы
        embedded = self.embedding(x)
        
        # Шаг 2: Пропускаем через LSTM (модель "думает")
        output, hidden = self.lstm(embedded, hidden)
        
        # Шаг 3: Предсказываем следующий символ
        logits = self.fc(output)
        
        return logits, hidden
    
    def generate(self, start_text, length=50, temperature=0.8):
        """Генерируем новый текст!"""
        self.eval()
        
        # Превращаем начало в числа
        current = torch.tensor([[char_to_idx[ch] for ch in start_text]])
        hidden = None
        result = list(start_text)
        
        with torch.no_grad():
            for _ in range(length):
                # Получаем предсказания
                logits, hidden = self.forward(current, hidden)
                
                # Берём предсказание для последнего символа
                last_logits = logits[0, -1] / temperature
                
                # Превращаем в вероятности
                probs = torch.softmax(last_logits, dim=-1)
                
                # Выбираем следующий символ (с сюрпризом!)
                next_idx = torch.multinomial(probs, 1).item()
                next_char = idx_to_char[next_idx]
                
                result.append(next_char)
                current = torch.tensor([[next_idx]])
        
        return ''.join(result)
    

def explore_generation(model):
    """Исследуем, как работает наша LLM"""
    
    print("\n" + "="*50)
    print("🤖 НАША LLM В ДЕЙСТВИИ!")
    print("="*50 + "\n")
    
    # Тест 1: Разные начала
    starters = ["the cat", "once upon", "every day", "one day"]
    
    print("📖 ТЕСТ 1: Разные начала фраз")
    print("-" * 40)
    for starter in starters:
        generated = model.generate(starter, length=40, temperature=0.7)
        print(f"Начало: '{starter}'")
        print(f"Результат: '{generated}'\n")
    
    # Тест 2: Влияние температуры (креативности)
    print("\n🎨 ТЕСТ 2: Как температура влияет на креативность")
    print("-" * 40)
    
    test_start = "the cat"
    temperatures = [0.3, 0.7, 1.2]
    
    for temp in temperatures:
        generated = model.generate(test_start, length=50, temperature=temp)
        if temp < 0.5:
            style = "❄️ Консервативный (мало сюрпризов)"
        elif temp > 1.0:
            style = "🔥 Креативный (много сюрпризов)"
        else:
            style = "⚖️ Сбалансированный"
        
        print(f"{style} (T={temp}):")
        print(f"'{generated}'\n")
    
    # Тест 3: Что "думает" модель?
    # print("\n🧠 ТЕСТ 3: Заглянем в "мозг" модели")
    # print("-" * 40)
    
    model.eval()
    with torch.no_grad():
        # Берём начало фразы
        test_input = "the cat was"
        input_tensor = torch.tensor([[char_to_idx[ch] for ch in test_input]])
        
        # Получаем предсказания
        logits, _ = model(input_tensor)
        last_logits = logits[0, -1]
        
        # Топ-5 самых вероятных символов
        probs = torch.softmax(last_logits, dim=-1)
        top_probs, top_indices = torch.topk(probs, 5)
        
        print(f"Для начала '{test_input}', модель думает что дальше:")
        for i, (prob, idx) in enumerate(zip(top_probs, top_indices), 1):
            char = idx_to_char[idx.item()]
            print(f"  {i}. '{char}' с вероятностью {prob.item():.2%}")
def train_model(model, data, epochs=500, seq_length=30, lr=0.01):
    """Обучаем модель предсказывать следующий символ"""
    
    # Функция ошибки и оптимизатор
    criterion = nn.CrossEntropyLoss()
    optimizer = optim.Adam(model.parameters(), lr=lr)
    
    losses = []
    
    print("🚀 Начинаем обучение...")
    
    for epoch in range(epochs):
        total_loss = 0
        num_batches = 0
        
        # Создаём обучающие примеры
        for i in range(0, len(data) - seq_length, seq_length):
            # Вход: последовательность символов
            inputs = torch.tensor(data[i:i+seq_length]).unsqueeze(0)
            # Цель: следующий символ после каждого
            targets = torch.tensor(data[i+1:i+seq_length+1]).unsqueeze(0)
            
            # Обучение
            optimizer.zero_grad()
            outputs, _ = model(inputs)
            loss = criterion(outputs.view(-1, vocab_size), targets.view(-1))
            loss.backward()
            optimizer.step()
            
            total_loss += loss.item()
            num_batches += 1
        
        avg_loss = total_loss / num_batches
        losses.append(avg_loss)
        
        # Показываем прогресс
        if epoch % 50 == 0:
            print(f"📚 Эпоха {epoch}, Ошибка: {avg_loss:.4f}")
            
            # Демонстрация генерации
            if epoch % 100 == 0:
                sample = model.generate("the cat", length=30, temperature=0.8)
                print(f"📝 Пример генерации: '{sample}'\n")
    
    return losses
def model_battle():
    """Соревнование между разными архитектурами"""
    
    print("\n" + "="*50)
    print("🏆 БИТВА LLM!")
    print("="*50 + "\n")
    
    models_to_test = {
        "Маленькая (16 нейронов)": MiniLLM(vocab_size, 16, 32, 1),
        "Средняя (64 нейрона)": MiniLLM(vocab_size, 32, 64, 2),
        "Большая (128 нейронов)": MiniLLM(vocab_size, 64, 128, 3)
    }
    
    results = {}
    test_prompt = "the cat"
    
    for name, m in models_to_test.items():
        print(f"📊 Тестируем {name}...")
        # Быстрое обучение (только 50 эпох для демонстрации)
        m.train()
        optimizer = optim.Adam(m.parameters(), lr=0.01)
        criterion = nn.CrossEntropyLoss()
        
        for epoch in range(50):
            # Обучаем на коротком тексте
            inputs = torch.tensor(data[:-1]).unsqueeze(0)
            targets = torch.tensor(data[1:]).unsqueeze(0)
            optimizer.zero_grad()
            outputs, _ = m(inputs[:, :50])
            loss = criterion(outputs.view(-1, vocab_size), targets[:, :50].view(-1))
            loss.backward()
            optimizer.step()
        
        # Генерируем текст
        generated = m.generate(test_prompt, length=30, temperature=0.7)
        results[name] = generated
        print(f"✅ Результат: '{generated}'\n")
    
    # Голосование студентов
    print("👥 А теперь голосуйте! Какая модель сгенерировала самый осмысленный текст?")
    for name, text in results.items():
        print(f"\n{name}:")
        print(f"  '{text}'")

model = MiniLLM(vocab_size, embedding_dim=32, hidden_dim=64, num_layers=2)
losses = train_model(model, data, epochs=300, seq_length=20)
import matplotlib.pyplot as plt
plt.figure(figsize=(10, 5))
plt.plot(losses)
plt.title('Как учится наша LLM')
plt.xlabel('Эпоха обучения')
plt.ylabel('Ошибка (чем меньше, тем лучше)')
plt.grid(True)
plt.show()
explore_generation()
model_battle()