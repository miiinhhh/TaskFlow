import { useEffect, useMemo, useState } from "react";

const API_URL = "http://localhost:5284/api";

function getStoredUser() {
  try {
    return JSON.parse(localStorage.getItem("taskflow_user"));
  } catch {
    return null;
  }
}

function dateText(value) {
  return value ? new Intl.DateTimeFormat("vi-VN").format(new Date(value)) : "-";
}

function FormError({ message }) {
  return message ? <p className="form-error">{message}</p> : null;
}

function Modal({ title, onClose, children }) {
  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <section className="modal" onMouseDown={(event) => event.stopPropagation()}>
        <div className="modal-heading">
          <h2>{title}</h2>
          <button className="close-button" type="button" onClick={onClose} aria-label="Đóng">×</button>
        </div>
        {children}
      </section>
    </div>
  );
}

function AuthPage({ request, onAuthenticated }) {
  const [tab, setTab] = useState("login");
  const [error, setError] = useState("");

  async function submitLogin(event) {
    event.preventDefault();
    setError("");
    const values = Object.fromEntries(new FormData(event.currentTarget));
    try {
      onAuthenticated(await request("/auth/login", { method: "POST", body: JSON.stringify(values) }));
    } catch (err) {
      setError(err.message);
    }
  }

  async function submitRegister(event) {
    event.preventDefault();
    setError("");
    const values = { ...Object.fromEntries(new FormData(event.currentTarget)), avatarUrl: null };
    try {
      onAuthenticated(await request("/auth/register", { method: "POST", body: JSON.stringify(values) }));
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-box">
        <div className="brand"><span className="brand-mark">TF</span><strong>TaskFlow</strong></div>
        <div className="auth-tabs">
          <button className={tab === "login" ? "active" : ""} onClick={() => { setTab("login"); setError(""); }}>Đăng nhập</button>
          <button className={tab === "register" ? "active" : ""} onClick={() => { setTab("register"); setError(""); }}>Đăng ký</button>
        </div>
        {tab === "login" ? (
          <form className="form-stack" onSubmit={submitLogin}>
            <label>Tên đăng nhập hoặc email<input name="usernameOrEmail" required autoComplete="username" /></label>
            <label>Mật khẩu<input name="password" type="password" required autoComplete="current-password" /></label>
            <button className="button primary">Đăng nhập</button>
          </form>
        ) : (
          <form className="form-stack" onSubmit={submitRegister}>
            <label>Họ và tên<input name="fullName" required maxLength="150" /></label>
            <label>Tên đăng nhập<input name="username" required minLength="3" maxLength="100" /></label>
            <label>Email<input name="email" type="email" required maxLength="255" /></label>
            <label>Mật khẩu<input name="password" type="password" required minLength="6" /></label>
            <button className="button primary">Tạo tài khoản</button>
          </form>
        )}
        <FormError message={error} />
      </section>
    </main>
  );
}

function ProjectForm({ request, onClose, onSaved }) {
  const [error, setError] = useState("");
  async function submit(event) {
    event.preventDefault();
    const values = Object.fromEntries(new FormData(event.currentTarget));
    values.description ||= null;
    values.startDate ||= null;
    values.endDate ||= null;
    try {
      await request("/projects", { method: "POST", body: JSON.stringify(values) });
      await onSaved();
      onClose();
    } catch (err) { setError(err.message); }
  }
  return <Modal title="Tạo dự án" onClose={onClose}>
    <form className="form-stack" onSubmit={submit}>
      <label>Tên dự án<input name="name" required maxLength="200" /></label>
      <label>Mô tả<textarea name="description" rows="3" maxLength="1000" /></label>
      <div className="form-row"><label>Ngày bắt đầu<input name="startDate" type="date" /></label><label>Ngày kết thúc<input name="endDate" type="date" /></label></div>
      <FormError message={error} />
      <div className="modal-actions"><button type="button" className="button" onClick={onClose}>Hủy</button><button className="button primary">Lưu</button></div>
    </form>
  </Modal>;
}

function TaskForm({ projects, statuses, priorities, request, onClose, onSaved }) {
  const [error, setError] = useState("");
  async function submit(event) {
    event.preventDefault();
    const values = Object.fromEntries(new FormData(event.currentTarget));
    const body = {
      ...values,
      projectId: Number(values.projectId), statusId: Number(values.statusId), priorityId: Number(values.priorityId),
      description: values.description || null,
      startDate: values.startDate ? new Date(values.startDate).toISOString() : null,
      dueDate: values.dueDate ? new Date(values.dueDate).toISOString() : null,
      estimatedHours: values.estimatedHours ? Number(values.estimatedHours) : null,
      actualHours: null
    };
    try {
      await request("/tasks", { method: "POST", body: JSON.stringify(body) });
      await onSaved();
      onClose();
    } catch (err) { setError(err.message); }
  }
  return <Modal title="Tạo công việc" onClose={onClose}>
    <form className="form-stack" onSubmit={submit}>
      <label>Dự án<select name="projectId" required>{projects.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label>Tiêu đề<input name="title" required maxLength="200" /></label>
      <label>Mô tả<textarea name="description" rows="3" maxLength="4000" /></label>
      <div className="form-row">
        <label>Trạng thái<select name="statusId" required>{statuses.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
        <label>Độ ưu tiên<select name="priorityId" required>{priorities.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      </div>
      <div className="form-row"><label>Ngày bắt đầu<input name="startDate" type="datetime-local" /></label><label>Hạn hoàn thành<input name="dueDate" type="datetime-local" /></label></div>
      <label>Số giờ dự kiến<input name="estimatedHours" type="number" min="0" step="0.5" /></label>
      <FormError message={error} />
      <div className="modal-actions"><button type="button" className="button" onClick={onClose}>Hủy</button><button className="button primary">Lưu</button></div>
    </form>
  </Modal>;
}

function MembersModal({ project, canManage, request, onClose, refreshProjects }) {
  const [members, setMembers] = useState([]);
  const [error, setError] = useState("");
  const load = async () => setMembers(await request(`/projects/${project.id}/members`));
  useEffect(() => { load().catch((err) => setError(err.message)); }, [project.id]);

  async function add(event) {
    event.preventDefault();
    setError("");
    const userId = Number(new FormData(event.currentTarget).get("userId"));
    try {
      await request(`/projects/${project.id}/members`, { method: "POST", body: JSON.stringify({ userId }) });
      event.currentTarget.reset();
      await Promise.all([load(), refreshProjects()]);
    } catch (err) { setError(err.message); }
  }

  async function remove(userId) {
    if (!confirm("Xóa thành viên này khỏi dự án?")) return;
    try {
      await request(`/projects/${project.id}/members/${userId}`, { method: "DELETE" });
      await Promise.all([load(), refreshProjects()]);
    } catch (err) { setError(err.message); }
  }

  return <Modal title={`Thành viên - ${project.name}`} onClose={onClose}>
    {canManage && <form className="inline-form" onSubmit={add}><input name="userId" type="number" min="1" required placeholder="Nhập User ID" /><button className="button primary">Thêm</button></form>}
    <FormError message={error} />
    <div className="member-list">{members.length ? members.map((member) => <div className="member-item" key={member.userId}><div><strong>{member.fullName}</strong><small>{member.username} · {member.roleName}</small></div>{canManage && <button className="text-button danger" onClick={() => remove(member.userId)}>Xóa</button>}</div>) : <p className="empty">Chưa có thành viên.</p>}</div>
  </Modal>;
}

function AssignModal({ task, request, onClose }) {
  const [members, setMembers] = useState([]);
  const [error, setError] = useState("");
  useEffect(() => { request(`/projects/${task.projectId}/members`).then(setMembers).catch((err) => setError(err.message)); }, [task.projectId]);
  async function submit(event) {
    event.preventDefault();
    try {
      const userId = Number(new FormData(event.currentTarget).get("userId"));
      await request(`/tasks/${task.id}/assignment`, { method: "PUT", body: JSON.stringify({ userId }) });
      onClose();
    } catch (err) { setError(err.message); }
  }
  return <Modal title="Giao công việc" onClose={onClose}><form className="form-stack" onSubmit={submit}><label>Thành viên<select name="userId" required>{members.map((member) => <option key={member.userId} value={member.userId}>{member.fullName} ({member.username})</option>)}</select></label><FormError message={error} /><div className="modal-actions"><button type="button" className="button" onClick={onClose}>Hủy</button><button className="button primary">Giao việc</button></div></form></Modal>;
}

export default function App() {
  const [token, setToken] = useState(localStorage.getItem("taskflow_token"));
  const [user, setUser] = useState(getStoredUser);
  const [view, setView] = useState("dashboard");
  const [projects, setProjects] = useState([]);
  const [tasks, setTasks] = useState([]);
  const [statuses, setStatuses] = useState([]);
  const [priorities, setPriorities] = useState([]);
  const [modal, setModal] = useState(null);
  const [notice, setNotice] = useState("");
  const [projectFilter, setProjectFilter] = useState("");
  const [search, setSearch] = useState("");

  const canManage = ["Admin", "Manager"].includes(user?.roleName);

  function logout() {
    localStorage.removeItem("taskflow_token");
    localStorage.removeItem("taskflow_user");
    setToken(null); setUser(null); setProjects([]); setTasks([]);
  }

  async function request(path, options = {}) {
    const headers = { ...(options.headers || {}) };
    if (options.body) headers["Content-Type"] = "application/json";
    const activeToken = localStorage.getItem("taskflow_token");
    if (activeToken) headers.Authorization = `Bearer ${activeToken}`;
    const response = await fetch(`${API_URL}${path}`, { ...options, headers });
    const type = response.headers.get("content-type") || "";
    const data = response.status === 204 ? null : type.includes("json") ? await response.json() : await response.text();
    if (!response.ok) {
      if (response.status === 401 && activeToken) logout();
      const message = typeof data === "string" ? data : data?.message || data?.title || "Không thể thực hiện yêu cầu.";
      throw new Error(message.replace(/^"|"$/g, ""));
    }
    return data;
  }

  async function loadData() {
    const [projectData, taskData, statusData, priorityData] = await Promise.all([request("/projects"), request("/tasks"), request("/statuses"), request("/priorities")]);
    setProjects(projectData); setTasks(taskData); setStatuses(statusData); setPriorities(priorityData);
  }

  useEffect(() => {
    if (token && user) loadData().catch((err) => setNotice(err.message));
  }, [token]);

  function authenticated(login) {
    localStorage.setItem("taskflow_token", login.accessToken);
    localStorage.setItem("taskflow_user", JSON.stringify(login.user));
    setUser(login.user); setToken(login.accessToken);
  }

  const visibleTasks = useMemo(() => tasks.filter((task) => (!projectFilter || task.projectId === Number(projectFilter)) && (!search || task.title.toLowerCase().includes(search.toLowerCase()))), [tasks, projectFilter, search]);
  const completedIds = new Set(statuses.filter((item) => item.isCompleted).map((item) => item.id));
  const recentTasks = [...tasks].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 5);
  const displayName = user?.username || user?.fullName || `User #${user?.id}`;

  async function deleteTask(id) {
    if (!confirm("Xóa công việc này?")) return;
    try { await request(`/tasks/${id}`, { method: "DELETE" }); await loadData(); } catch (err) { setNotice(err.message); }
  }

  if (!token || !user) return <AuthPage request={request} onAuthenticated={authenticated} />;

  return <div className="app-shell">
    <header className="topbar"><div className="brand"><span className="brand-mark">TF</span><strong>TaskFlow</strong></div><div className="user-area"><span>{displayName}</span><span className="role-badge">{user.roleName}</span><button className="button small" onClick={logout}>Đăng xuất</button></div></header>
    <div className="layout">
      <nav className="sidebar">{[["dashboard", "Tổng quan"], ["projects", "Dự án"], ["tasks", "Công việc"]].map(([key, label]) => <button key={key} className={view === key ? "active" : ""} onClick={() => setView(key)}>{label}</button>)}</nav>
      <main className="content">
        {notice && <div className="notice"><span>{notice}</span><button onClick={() => setNotice("")}>×</button></div>}
        {view === "dashboard" && <section>
          <PageHeading title="Tổng quan" subtitle={`Xin chào, ${displayName}`}>{canManage && <button className="button primary" onClick={() => setModal({ type: "task" })}>+ Tạo công việc</button>}</PageHeading>
          <div className="summary-grid"><Summary label="Dự án" value={projects.length} /><Summary label="Công việc" value={tasks.length} /><Summary label="Đã hoàn thành" value={tasks.filter((item) => completedIds.has(item.statusId)).length} /></div>
          <section className="panel"><h2>Công việc gần đây</h2>{recentTasks.length ? recentTasks.map((task) => <div className="simple-item" key={task.id}><div><strong>{task.title}</strong><small>{task.projectName}</small></div><span className="status">{task.statusName}</span></div>) : <p className="empty">Chưa có công việc.</p>}</section>
        </section>}
        {view === "projects" && <section>
          <PageHeading title="Dự án" subtitle="Danh sách dự án bạn có thể truy cập">{canManage && <button className="button primary" onClick={() => setModal({ type: "project" })}>+ Tạo dự án</button>}</PageHeading>
          <div className="panel table-panel"><table><thead><tr><th>Tên dự án</th><th>Người tạo</th><th>Thời gian</th><th>Thành viên</th><th></th></tr></thead><tbody>{projects.length ? projects.map((project) => <tr key={project.id}><td><strong>{project.name}</strong><small>{project.description || "Không có mô tả"}</small></td><td>{project.ownerName || "-"}</td><td>{dateText(project.startDate)} - {dateText(project.endDate)}</td><td>{project.memberCount}</td><td className="actions"><button className="text-button" onClick={() => setModal({ type: "members", project })}>Thành viên</button></td></tr>) : <tr><td colSpan="5" className="empty">Chưa có dự án.</td></tr>}</tbody></table></div>
        </section>}
        {view === "tasks" && <section>
          <PageHeading title="Công việc" subtitle="Theo dõi công việc trong các dự án">{canManage && <button className="button primary" onClick={() => setModal({ type: "task" })}>+ Tạo công việc</button>}</PageHeading>
          <div className="filters"><select value={projectFilter} onChange={(event) => setProjectFilter(event.target.value)}><option value="">Tất cả dự án</option>{projects.map((project) => <option key={project.id} value={project.id}>{project.name}</option>)}</select><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Tìm theo tên công việc" /></div>
          <div className="panel table-panel"><table><thead><tr><th>Công việc</th><th>Dự án</th><th>Trạng thái</th><th>Ưu tiên</th><th>Hạn</th>{canManage && <th></th>}</tr></thead><tbody>{visibleTasks.length ? visibleTasks.map((task) => <tr key={task.id}><td><strong>{task.title}</strong><small>#{task.id}</small></td><td>{task.projectName}</td><td><span className={completedIds.has(task.statusId) ? "status done" : "status"}>{task.statusName}</span></td><td>{task.priorityName}</td><td>{dateText(task.dueDate)}</td>{canManage && <td className="actions"><button className="text-button" onClick={() => setModal({ type: "assign", task })}>Giao việc</button><button className="text-button danger" onClick={() => deleteTask(task.id)}>Xóa</button></td>}</tr>) : <tr><td colSpan={canManage ? 6 : 5} className="empty">Không có công việc phù hợp.</td></tr>}</tbody></table></div>
        </section>}
      </main>
    </div>
    {modal?.type === "project" && <ProjectForm request={request} onClose={() => setModal(null)} onSaved={loadData} />}
    {modal?.type === "task" && <TaskForm projects={projects} statuses={statuses} priorities={priorities} request={request} onClose={() => setModal(null)} onSaved={loadData} />}
    {modal?.type === "members" && <MembersModal project={modal.project} canManage={canManage} request={request} onClose={() => setModal(null)} refreshProjects={loadData} />}
    {modal?.type === "assign" && <AssignModal task={modal.task} request={request} onClose={() => setModal(null)} />}
  </div>;
}

function PageHeading({ title, subtitle, children }) {
  return <div className="page-heading"><div><h1>{title}</h1><p>{subtitle}</p></div>{children}</div>;
}

function Summary({ label, value }) {
  return <article className="summary-item"><span>{label}</span><strong>{value}</strong></article>;
}
