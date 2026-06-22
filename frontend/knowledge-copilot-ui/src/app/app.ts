
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Component, signal, ChangeDetectorRef } from '@angular/core';

interface SourceCitation {
  documentName: string;
  chunkIndex: number;
  excerptText: string;
  relevanceScore: number;
}

interface ChatMessage {
  role: 'user' | 'assistant';
  text: string;
  sources?: SourceCitation[];
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly apiUrl = 'http://localhost:5119';

  selectedFile: File | null = null;
  isUploading = false;
  uploadMessage = '';

  currentQuestion = '';
  isAsking = false;
  messages: ChatMessage[] = [];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.uploadMessage = '';
    }
  }

  uploadFile() {
    if (!this.selectedFile) return;

    this.isUploading = true;
    this.uploadMessage = '';

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.http.post<any>(`${this.apiUrl}/api/documents/upload`, formData).subscribe({
      next: (res) => {
        this.uploadMessage = `✅ Uploaded "${res.fileName}" — ${res.totalChunks} chunks indexed.`;
        this.isUploading = false;
        this.selectedFile = null;
      },
      error: (err) => {
        this.uploadMessage = `❌ Error: ${err.error?.error || 'Upload failed.'}`;
        this.isUploading = false;
      }
    });
  }

  askQuestion() {
    if (!this.currentQuestion.trim()) return;

    const question = this.currentQuestion;
this.messages = [...this.messages, { role: 'user', text: question }];
    this.currentQuestion = '';
    this.isAsking = true;

   this.http.post<any>(`${this.apiUrl}/api/query`, { question }).subscribe({
      next: (res) => {
        console.log('RESPONSE RECEIVED:', res);
        this.messages = [...this.messages, {
          role: 'assistant',
          text: res.answer,
          sources: res.sources
        }];
        this.isAsking = false;
this.cdr.detectChanges();
        console.log('MESSAGES NOW:', this.messages);
      },
      error: (err) => {
        this.messages = [...this.messages, {
  role: 'assistant',
  text: `❌ Error: ${err.error?.error || 'Something went wrong.'}`
}];
        this.isAsking = false;
this.cdr.detectChanges();
      }
    });
  }
}