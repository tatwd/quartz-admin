import React, { Component, useRef } from "react";
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
} from "reactstrap";

export function Dashboard() {
  const modalRef = useRef(null);
  const jobsTableRef = useRef(null);

  const handleEdit = (item) => {
    modalRef.current.setState({
      setting: item,
    });
    modalRef.current.toggleShow();
  };

  const handleDelete = () => {
    // get selected items
    const selectedIds = jobsTableRef.current.state.jobs
      .filter((i) => i.selected)
      .map((i) => i.id);
    console.log(selectedIds);

    if (!selectedIds.length)
      return window.alert("Please select job(s) that you want to delete!");

    if (window.confirm("Sure to delete these jobs?")) {
      fetch("api/jobs/settings/delete", {
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
    }
  };

  const handleModalSubmitSuccess = () => {
    jobsTableRef.current.fetchData();
  };

  return (
    <div>
      <Button color="info" onClick={() => modalRef.current.toggleShow()}>
        New Job
      </Button>{" "}
      <Button color="danger" onClick={handleDelete}>
        Delete Job(s)
      </Button>
      <JobsTable ref={jobsTableRef} onEdit={handleEdit} />
      <MyAlertModal ref={modalRef} onSubmitSuccess={handleModalSubmitSuccess} />
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
        console.log(res);
        if (res.code === 0) {
          this.setState({
            jobs: res.detail,
          });
        } else window.alert(res.message || "Server error!");
      })
      .catch((err) => alert("Network error!"))
      .finally(() => this.setState({ loading: false }));
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
                <th className="">
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
                        }}
                      />
                      All
                    </Label>
                  </FormGroup>
                </th>
                <th>Job Name</th>
                <th>Job Group</th>
                <th>Job Description</th>
                <th>State</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {jobs.map((item, idx) => (
                <tr key={idx}>
                  <th scope="row">
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
                          }}
                        />
                        #{item.id}
                      </Label>
                    </FormGroup>
                  </th>
                  <td>{item.jobName}</td>
                  <td>{item.jobGroup}</td>
                  <td>{item.jobDesc}</td>
                  <td>{item.state}</td>
                  <td style={{ minWidth: "200px" }}>
                    <Button
                      size="sm"
                      color="link"
                      className="mb-2 mb-md-0"
                      onClick={() => {}}
                    >
                      Logs
                    </Button>{" "}
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
                      color="warning"
                      className="mb-2 mb-md-0"
                      onClick={() => {}}
                    >
                      Stop
                    </Button>{" "}
                    <Button
                      size="sm"
                      color="success"
                      className="mb-2 mb-md-0"
                      onClick={() => {}}
                    >
                      Start
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

const initSetting = () => ({
  id: undefined,
  jobName: "",
  jobGroup: "",
  jobDesc: "",
  triggerType: 0,
  triggerExpr: "",
  httpApiUrl: "",
  httpMethod: "GET",
  httpContentType: "application/x-www-form-urlencoded",
  httpBody: "",
  // TODO: others properties
});

class MyAlertModal extends Component {
  constructor(props, ref) {
    super(props);

    this.state = {
      show: false,
      setting: initSetting(),
      triggerExprInvalidMessage: "",
    };
  }

  toggleShow = () => {
    if (this.state.show) {
      this.setState({
        setting: initSetting(),
      });
    }
    this.setState({
      show: !this.state.show,
    });
  };

  toggleChange = (event) => {
    const target = event.target;
    const value = target.type === "checkbox" ? target.checked : target.value;
    const name = target.name;

    const newSetting = this.state.setting;
    newSetting[name] = value;
    this.setState({
      setting: newSetting,
    });
  };

  handleSubmit = (event) => {
    console.log(event);
    var newSetting = this.state.setting;
    newSetting.triggerType = parseInt(newSetting.triggerType);
    fetch("api/jobs/settings", {
      method: "POST",
      body: JSON.stringify(this.state.setting),
      headers: { "Content-Type": "application/json" },
    })
      .then((res) => res.json())
      .then((res) => {
        console.log(res);
        if (res.code === 0) {
          this.toggleShow();
          this.props.onSubmitSuccess();
        } else window.alert(res.message || "Server error!");
      })
      .catch((err) => window.alert("Network error!"));
  };

  render() {
    return (
      <Modal isOpen={this.state.show} toggle={this.toggleShow}>
        <ModalHeader>Setting</ModalHeader>
        <ModalBody>
          <Form>
            <p>任务设置</p>
            <FormGroup>
              <Input
                placeholder="Job name"
                name="jobName"
                value={this.state.setting.jobName}
                onChange={this.toggleChange}
                required
              />
            </FormGroup>
            <FormGroup>
              <Input
                placeholder="Job Group"
                name="jobGroup"
                value={this.state.setting.jobGroup}
                onChange={this.toggleChange}
                required
              />
            </FormGroup>
            <FormGroup>
              <Input
                type="textarea"
                placeholder="Job Description"
                name="jobDesc"
                value={this.state.setting.jobDesc}
                onChange={this.toggleChange}
                required
              />
            </FormGroup>
            <FormGroup>
              <Input
                placeholder="Trigger Type"
                name="triggerType"
                type="select"
                value={this.state.setting.triggerType}
                onChange={this.toggleChange}
                required
              >
                <option value={0}>Simple Trigger</option>
                <option value={1}>Cron Trigger</option>
              </Input>
            </FormGroup>
            <FormGroup>
              <Input
                placeholder="Trigger Expression"
                name="triggerExpr"
                value={this.state.setting.triggerExpr || ""}
                onChange={this.toggleChange}
                onBlur={(event) => {
                  const expr = event.target.value;
                  const type = this.state.setting.triggerType;
                  if (expr) {
                    fetch(`api/jobs/validexpr?expr=${expr}&type=${type}`)
                      .then((res) => res.json())
                      .then((res) => {
                        console.log(res);
                        this.setState({
                          triggerExprInvalidMessage:
                            res.code === 0 ? "ok" : res.message,
                        });
                      });
                  } else
                    this.setState({
                      triggerExprInvalidMessage: "不能为空",
                    });
                }}
                required
                valid={this.state.triggerExprInvalidMessage === "ok"}
                invalid={
                  !!this.state.triggerExprInvalidMessage &&
                  this.state.triggerExprInvalidMessage !== "ok"
                }
              />
              <FormFeedback>
                {this.state.triggerExprInvalidMessage || "不能为空"}
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
                value={this.state.setting.httpApiUrl}
                onChange={this.toggleChange}
                required
              />
            </FormGroup>
            <FormGroup>
              <Input
                type="select"
                placeholder="HTTP Method"
                name="httpMethod"
                value={this.state.setting.httpMethod}
                onChange={this.toggleChange}
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
                value={this.state.setting.httpContentType || ""}
                onChange={this.toggleChange}
              />
            </FormGroup>
            <FormGroup>
              <Input
                type="textarea"
                placeholder="HTTP Body"
                name="httpBody"
                value={this.state.setting.httpBody || ""}
                onChange={this.toggleChange}
              />
            </FormGroup>
          </Form>
        </ModalBody>
        <ModalFooter>
          <Button onClick={this.handleSubmit} color="primary">
            Submit
          </Button>
          <Button onClick={this.toggleShow}>Cancel</Button>
        </ModalFooter>
      </Modal>
    );
  }
}
