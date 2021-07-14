import React, {
  Component,
  forwardRef,
  useRef,
  useState,
  useImperativeHandle,
} from "react";
import {
  Button,
  Form,
  FormGroup,
  FormFeedback,
  FormText,
  Input,
  Label,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
  Table,
  CustomInput,
  Spinner,
} from "reactstrap";

export function Dashboard() {
  const modalRef = useRef(null);
  const jobsTableRef = useRef(null);
  const [hiddenButtons, setHiddenButtons] = useState(true);
  const [autoRefresh, setAutoRefresh] = useState(false);
  const [autoRefreshTaskId, setAutoRefreshTaskId] = useState(0);

  const handleEdit = (item) => {
    // modalRef.current.setSetting(item);
    modalRef.current.toggleShow(item);
  };

  const handleBatchStart = () => {};

  const handleBatchPause = () => {
    // get selected items
    const selectedIds = jobsTableRef.current.state.jobs
      .filter((i) => i.selected)
      .map((i) => i.id);
    console.log(selectedIds);
    if (!selectedIds.length)
      return window.alert("Please select job(s) that you want to pause!");
    if (!window.confirm("Sure to pause these jobs?")) {
      return;
    }
    jobsTableRef.current.pauseJobs(selectedIds);
  };

  const handleBatchDelete = () => {
    // get selected items
    const selectedIds = jobsTableRef.current.state.jobs
      .filter((i) => i.selected)
      .map((i) => i.id);
    console.log(selectedIds);

    if (!selectedIds.length)
      return window.alert("Please select job(s) that you want to delete!");

    if (!window.confirm("Sure to delete these jobs?")) {
      return;
    }
    fetch("api/jobs/delete", {
      method: "POST",
      body: JSON.stringify(selectedIds),
      headers: {
        "Content-Type": "application/json; charset=utf-8",
      },
    })
      .then((res) => res.json())
      .then((res) => {
        console.log(res);
        if (res.code === 0) {
          jobsTableRef.current.fetchData();
        } else window.alert("Server error!");
      })
      .catch((err) => window.alert("Network error!"));
  };

  const refetchData = () => {
    jobsTableRef.current.state.selectedAll = false;
    jobsTableRef.current.fetchData();
  };

  const startAutoRefreshData = () => {
    const taskId = setInterval(() => {
      jobsTableRef.current.fetchData();
    }, 2 * 1000);
    setAutoRefreshTaskId(taskId);
  };

  const stopAutoRefreshData = () => {
    if (!autoRefreshTaskId) return;
    window.clearInterval(autoRefreshTaskId);
    setAutoRefreshTaskId(0);
  };

  return (
    <div>
      <Button color="info" onClick={() => modalRef.current.toggleShow()}>
        New Job
      </Button>{" "}
      <Button onClick={() => refetchData()}>Refresh</Button>{" "}
      {hiddenButtons ? (
        <></>
      ) : (
        <>
          <Button color="success" onClick={handleBatchStart}>
            Start
          </Button>{" "}
          <Button color="warning" onClick={handleBatchPause}>
            Pause
          </Button>{" "}
          <Button color="danger" onClick={handleBatchDelete}>
            Delete
          </Button>
        </>
      )}
      <div className="mt-3">
        <CustomInput
          type="switch"
          label="auto refresh"
          id="auto-refresh"
          checked={autoRefresh}
          onChange={(event) => {
            var checked = !autoRefresh;
            setAutoRefresh(checked);
            if (checked) {
              startAutoRefreshData();
            } else {
              stopAutoRefreshData();
            }
          }}
          inline={true}
        />
        {autoRefresh ? <Spinner size="sm" color="primary" /> : <></>}
      </div>
      <JobsTable
        ref={jobsTableRef}
        onEdit={handleEdit}
        onSelect={(selected, jobs) => {
          console.log(jobs.map((i) => i.selected));
          const hasSelectedItem = jobs.some((i) => i.selected);
          console.log(hasSelectedItem);
          setHiddenButtons(!hasSelectedItem);
        }}
      />
      <MyAlertModal ref={modalRef} onSubmitSuccess={() => refetchData()} />
    </div>
  );
}

class JobsTable extends Component {
  constructor(props, ref) {
    super(props);

    this.state = {
      loading: true,
      jobs: [],
      selectedAll: false,
    };
  }

  componentDidMount = () => {
    this.fetchData();
  };

  fetchData = () => {
    fetch("api/jobs/settings?page=1&limit=10")
      .then((res) => res.json())
      .then((res) => {
        // console.log(res);
        if (res.code === 0) {
          this.setState({
            jobs: res.detail,
          });
        } else window.alert(res.message || "Server error!");
      })
      .catch((err) => alert("Network error!"))
      .finally(() => this.setState({ loading: false }));
  };

  createJobTrigger = (jobId) => {
    fetch(`api/jobs/${jobId}/triggers`, {
      method: "POST",
    })
      .then((res) => res.json())
      .then((res) => {
        console.log(res);
        if (res.code === 0) {
          setTimeout(() => this.fetchData(), 500);
        } else window.alert(res.message || "Server error!");
      })
      .catch((err) => alert("Network error!"));
  };

  pauseJobs = (jobIds) => {
    fetch("api/jobs/pause", {
      method: "POST",
      body: JSON.stringify(jobIds),
      headers: {
        "Content-Type": "application/json; charset=utf-8",
      },
    })
      .then((res) => res.json())
      .then((res) => {
        console.log(res);
        if (res.code === 0) {
          setTimeout(() => this.fetchData(), 500);
        } else window.alert("Server error!");
      })
      .catch((err) => window.alert("Network error!"));
  };

  render() {
    const { loading, jobs, selectedAll } = this.state;
    return (
      <div className="mt-3">
        {loading ? (
          <i>Loading ...</i>
        ) : jobs.length ? (
          <Table striped responsive>
            <thead>
              <tr>
                <th>
                  <FormGroup check>
                    <Label check>
                      <Input
                        type="checkbox"
                        id="selectAll"
                        checked={selectedAll}
                        onChange={(event) => {
                          const checked = event.target.checked;
                          this.setState({ selectedAll: !selectedAll });
                          this.setState({
                            jobs: jobs.map((i) => {
                              if (i.selected !== checked) {
                                i.selected = checked;
                              }
                              return i;
                            }),
                          });
                          this.props.onSelect(checked, jobs);
                        }}
                      />
                      All
                    </Label>
                  </FormGroup>
                </th>
                <th>Job Name</th>
                <th>Job Group</th>
                <th>Job Description</th>
                <th>Startup Type</th>
                <th>State</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {jobs.map((item, idx) => (
                <tr key={idx}>
                  <th scope="row" style={{ minWidth: "15px" }}>
                    <FormGroup check>
                      <Label check>
                        <Input
                          type="checkbox"
                          id={"item" + item.id}
                          checked={!!item.selected}
                          value={idx}
                          onChange={(event) => {
                            const checked = event.target.checked;
                            const newJobs = jobs.slice();
                            newJobs[idx].selected = checked;
                            this.setState({ jobs: newJobs });
                            if (selectedAll)
                              this.setState({ selectedAll: false });
                            this.props.onSelect(checked, jobs);
                          }}
                        />
                        #{item.id}
                      </Label>
                    </FormGroup>
                  </th>
                  <td
                    style={{
                      minWidth: "100px",
                      maxWidth: "300px",
                      wordBreak: "break-word",
                    }}
                  >
                    {item.jobName}
                  </td>
                  <td
                    style={{
                      minWidth: "100px",
                      maxWidth: "200px",
                      wordBreak: "break-word",
                    }}
                  >
                    {item.jobGroup}
                  </td>
                  <td style={{ minWidth: "100px", wordBreak: "break-word" }}>
                    {item.jobDesc}
                  </td>
                  <td>{item.startupType}</td>
                  <td>{item.state}</td>
                  <td style={{ minWidth: "200px" }}>
                    {/*<Button*/}
                    {/*  size="sm"*/}
                    {/*  color="link"*/}
                    {/*  className="mb-2 mb-md-0"*/}
                    {/*  onClick={() => {}}*/}
                    {/*>*/}
                    {/*  Logs*/}
                    {/*</Button>{" "}*/}
                    <Button
                      size="sm"
                      color="primary"
                      className="mb-2 mb-md-0"
                      onClick={() => this.props.onEdit(item)}
                    >
                      Edit
                    </Button>{" "}
                    <Button
                      size="sm"
                      color="success"
                      className="mb-2 mb-md-0"
                      onClick={() => this.createJobTrigger(item.id)}
                    >
                      Start
                    </Button>{" "}
                    <Button
                      size="sm"
                      color="warning"
                      className="mb-2 mb-md-0"
                      onClick={() => this.pauseJobs([item.id])}
                    >
                      Pause
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <p>No jobs, please click button to create a one!</p>
        )}
      </div>
    );
  }
}

const MyAlertModal = forwardRef((props, ref) => {
  const [show, setShow] = useState(false);

  const initSetting = {
    id: undefined,
    jobName: "",
    jobGroup: "",
    jobDesc: "",
    triggerType: "Simple",
    triggerExpr: "",
    httpApiUrl: "",
    httpMethod: "GET",
    httpContentType: "application/x-www-form-urlencoded",
    httpBody: "",
    startupType: "Auto",
    // TODO: others properties
  };
  const [setting, setSetting] = useState(initSetting);
  const [triggerExprInvalidMessage, setTriggerExprInvalidMessage] =
    useState("");

  const toggleChange = (event) => {
    console.log(event);
    const target = event.target;
    const value = target.type === "checkbox" ? target.checked : target.value;
    const name = target.name;

    setting[name] = value;
    setSetting({ ...setting });
  };

  const handleSubmit = (event) => {
    fetch("api/jobs/settings", {
      method: "POST",
      body: JSON.stringify(setting),
      headers: { "Content-Type": "application/json" },
    })
      .then((res) => res.json())
      .then((res) => {
        console.log(res);
        if (res.code === 0) {
          setShow(false);
          props.onSubmitSuccess();
        } else window.alert(res.message || "Server error!");
      })
      .catch((err) => window.alert("Network error!"));
  };

  useImperativeHandle(
    ref,
    // out a object for parent component
    () => ({
      toggleShow: (editItem) => {
        setSetting(editItem || initSetting);
        setShow(true);
      },
    }),
    [initSetting]
  );

  return (
    <Modal isOpen={show} toggle={() => setShow(!show)}>
      <ModalHeader>Setting</ModalHeader>
      <ModalBody>
        <Form>
          {/*<p>任务设置</p>*/}
          <FormGroup>
            <Input
              placeholder="Job name"
              name="jobName"
              value={setting.jobName}
              onChange={toggleChange}
              required
            />
          </FormGroup>
          <FormGroup>
            <Input
              placeholder="Job Group"
              name="jobGroup"
              value={setting.jobGroup}
              onChange={toggleChange}
              required
            />
          </FormGroup>
          <FormGroup>
            <Input
              type="textarea"
              placeholder="Job Description"
              name="jobDesc"
              value={setting.jobDesc}
              onChange={toggleChange}
              required
            />
          </FormGroup>
          <FormGroup>
            <Input
              placeholder="Startup Type"
              name="startupType"
              type="select"
              value={setting.startupType}
              onChange={toggleChange}
              required
            >
              <option value={"Auto"}>Auto</option>
              <option value={"Manual"}>Manual</option>
            </Input>
          </FormGroup>
          <FormGroup>
            <Input
              placeholder="Trigger Type"
              name="triggerType"
              type="select"
              value={setting.triggerType}
              onChange={toggleChange}
              required
            >
              <option value={"Simple"}>Simple Trigger</option>
              <option value={"Corn"}>Cron Trigger</option>
            </Input>
          </FormGroup>
          <FormGroup>
            <Input
              placeholder="Trigger Expression"
              name="triggerExpr"
              value={setting.triggerExpr || ""}
              onChange={toggleChange}
              onBlur={(event) => {
                const expr = event.target.value;
                const type = setting.triggerType;
                if (expr) {
                  fetch(`api/jobs/validexpr?expr=${expr}&type=${type}`)
                    .then((res) => res.json())
                    .then((res) => {
                      console.log(res);
                      setTriggerExprInvalidMessage(
                        res.code === 0 ? "ok" : res.message
                      );
                    });
                } else setTriggerExprInvalidMessage("不能为空");
              }}
              required
              valid={triggerExprInvalidMessage === "ok"}
              invalid={
                !!triggerExprInvalidMessage &&
                triggerExprInvalidMessage !== "ok"
              }
            />
            <FormFeedback>
              {triggerExprInvalidMessage || "不能为空"}
            </FormFeedback>
            <FormText>
              <span>
                1. 选择<code>Simple Trigger</code>取值必须形如
                <code>启动时间点|间隔秒数|重复数</code>。举例，
                <code>2020-09-01 02:00|5|2</code>表示将在2020-09-01
                02:00启动任务并间隔5s重复2次（共3次执行）。
              </span>
              <br />
              <span>
                2. 选择<code>Cron Trigger</code>取值必须使用 Cron 表达式。
              </span>
            </FormText>
          </FormGroup>
          <p>接口设置</p>
          <FormGroup>
            <Input
              type="textarea"
              placeholder="HTTP API URL"
              name="httpApiUrl"
              value={setting.httpApiUrl}
              onChange={toggleChange}
              required
            />
          </FormGroup>
          <FormGroup>
            <Input
              type="select"
              placeholder="HTTP Method"
              name="httpMethod"
              value={setting.httpMethod}
              onChange={toggleChange}
              required
            >
              <option>GET</option>
              <option>POST</option>
            </Input>
          </FormGroup>
          <FormGroup>
            <Input
              placeholder="HTTP Content Type"
              name="httpContentType"
              value={setting.httpContentType || ""}
              onChange={toggleChange}
            />
          </FormGroup>
          <FormGroup>
            <Input
              type="textarea"
              placeholder="HTTP Body"
              name="httpBody"
              value={setting.httpBody || ""}
              onChange={toggleChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        <Button onClick={handleSubmit} color="primary">
          Submit
        </Button>
        <Button onClick={() => setShow(false)}>Cancel</Button>
      </ModalFooter>
    </Modal>
  );
});
