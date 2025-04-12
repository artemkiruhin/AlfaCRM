import './App.css';
import RootLayout from "./components/layout/RootLayout";
import NewsPage from "./pages/NewsPage/NewsPage";
import NewsPostPage from "./pages/newsPostPage/NewsPostPage";
import NewsEditPage from "./pages/newsEditPage/NewsEditPage";
import ChatListPage from "./pages/chatListPage/ChatListPage";
import ChatConversationPage from "./pages/chatConversationPage/ChatConversationPage";
import MyTicketPage from "./pages/myTicketPage/MyTicketPage";
import TicketDetailsPage from "./pages/TicketDetailsPage/TicketDetailsPage";
import SentTicketsPage from "./pages/SentTicketsPage/SentTicketsPage";
import NewsPostCreatePage from "./pages/NewsPostCreatePage/NewsPostCreatePage";
import TicketCreatePage from "./pages/TicketCreatePage/TicketCreatePage";
import UserListPage from "./pages/UserListPage/UserListPage";
import UserCreatePage from "./pages/UserCreatePage/UserCreatePage";
import UserEditPage from "./pages/UserEditPage/UserEditPage";
import DepartmentListPage from "./pages/DepartmentListPage/DepartmentListPage";
import {BrowserRouter, Routes, Route} from "react-router-dom";
import AuthLayout from "./components/layout/AuthLayout";
import {AuthPage} from "./pages/AuthPage/AuthPage";
import React from "react";

function App() {
  return (
    <div className="App">
        <BrowserRouter>
            <Routes>
                <Route path="/news" element={<AuthLayout><RootLayout page={<NewsPage />}/></AuthLayout>} />
                <Route path="*" element={<AuthLayout><RootLayout page={<NewsPage />}/></AuthLayout>} />
                <Route path="/news/:id" element={<AuthLayout><RootLayout page={<NewsPostPage />}/></AuthLayout>} />
                <Route path="/news/edit/:id" element={<AuthLayout><RootLayout page={<NewsEditPage />}/></AuthLayout>} />
                <Route path="/news/add" element={<AuthLayout><RootLayout page={<NewsPostCreatePage />}/></AuthLayout>} />

                <Route path="/chat" element={<AuthLayout><RootLayout page={<ChatListPage />}/></AuthLayout>} />
                <Route path="/chat/:id" element={<AuthLayout><RootLayout page={<ChatConversationPage />}/></AuthLayout>} />

                <Route path="/tickets/my" element={<AuthLayout><RootLayout page={<MyTicketPage type={0} />}/></AuthLayout>} />
                <Route path="/tickets/my/:id" element={<AuthLayout><RootLayout page={<TicketDetailsPage type={0} />}/></AuthLayout>} />
                <Route path="/tickets/sent" element={<AuthLayout><RootLayout page={<SentTicketsPage type={0} />}/></AuthLayout>} />
                <Route path="/tickets/create" element={<AuthLayout><RootLayout page={<TicketCreatePage type={0} />}/></AuthLayout>} />

                <Route path="/suggestions/my" element={<AuthLayout><RootLayout page={<MyTicketPage type={1} />}/></AuthLayout>} />
                <Route path="/suggestions/my/:id" element={<AuthLayout><RootLayout page={<TicketDetailsPage type={1} />}/></AuthLayout>} />
                <Route path="/suggestions/sent" element={<AuthLayout><RootLayout page={<SentTicketsPage type={1} />}/></AuthLayout>} />
                <Route path="/suggestions/create" element={<AuthLayout><RootLayout page={<TicketCreatePage type={1} />}/></AuthLayout>} />

                <Route path="/users" element={<AuthLayout><RootLayout page={<UserListPage />}/></AuthLayout>} />
                <Route path="/users/edit/:id" element={<AuthLayout><RootLayout page={<UserEditPage />}/></AuthLayout>} />
                <Route path="/users/create" element={<AuthLayout><RootLayout page={<UserCreatePage />}/></AuthLayout>} />

                <Route path="/departments" element={<AuthLayout><RootLayout page={<DepartmentListPage />}/></AuthLayout>} />

                <Route path="/login" element={<AuthPage/>}/>
            </Routes>
        </BrowserRouter>
    </div>
  );
}

export default App;
