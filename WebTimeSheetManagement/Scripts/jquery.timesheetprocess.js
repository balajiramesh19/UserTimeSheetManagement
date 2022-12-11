function Approvetimesheet() {
    if (confirm('Are you sure you want to Approve Timesheet?'))
    {
        if ($("#Comment").val() == '')
        {
            alert("Please Enter Comment");
            return false;
        }
        else {

            var TimeSheetModel =
                {
                    TimeSheetMasterID: $("#TimeSheetMasterID").val(),
                    Comment: $("#Comment").val(),
                };

            var url = '/timesheets/ShowAllTimeSheet/Approval';
            $.post(url, { TimeSheetApproval: TimeSheetModel }, function (data) {
                if (data) {
                    alert("Timesheet Approved Successfully");
                    window.location.href = "/timesheets/ShowAllTimeSheet/TimeSheet";
                    return true;
                }
                else {
                    alert("Something Went Wrong!");
                    return false;
                }
            });
        }
    }
    else {
        return false;
    }
}

function Rejecttimesheet() {

    if (confirm('Are you sure you want to Reject Timesheet?')) {
        if ($("#Comment").val() == '') {
            alert("Please Enter Comment");
            return false;
        }
        else {

            var TimeSheetModel =
               {
                   TimeSheetMasterID: $("#TimeSheetMasterID").val(),
                   Comment: $("#Comment").val(),
               };

            var url =  '/timesheets/ShowAllTimeSheet/Rejected';
            $.post(url, { TimeSheetApproval: TimeSheetModel }, function (data) {
                if (data) {
                    alert("Timesheet Rejected Successfully");
                    window.location.href = "/timesheets/ShowAllTimeSheet/TimeSheet";
                    return true;
                }
                else {
                    alert("Something Went Wrong!");
                }
            });

        }
    }
    else {
        return false;
    }
}